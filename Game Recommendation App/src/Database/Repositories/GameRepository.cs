using Game_Recommendation.Cli.Config;
using Game_Recommendation.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using static Game_Recommendation.Database.Services.RawgApiService;

namespace Game_Recommendation.Database.Repositories
{
    public class GameRepository
    {
        private readonly ConnectionPool _pool;

        // Base query chunks reused across multiple methods
        private const string BaseGameQuery = @"
            SELECT g.id, g.title, g.publisher, g.game_description, g.avg_rating, g.total_ratings,
                   GROUP_CONCAT(gr.genre_name ORDER BY gr.genre_name SEPARATOR ', ') AS genres
            FROM games g
            LEFT JOIN game_genres gg ON g.id = gg.game_id
            LEFT JOIN genres gr ON gg.genre_id = gr.id";

        private const string BaseGameQueryGroup = "GROUP BY g.id, g.title, g.publisher, g.game_description, g.avg_rating, g.total_ratings";

        private List<Game> _cachedGames;
        private DateTime _cacheExpiry;

        public GameRepository(ConnectionPool pool)
        {
            _pool = pool;
        }

        #region Saving

        // Returns the new game's id via LAST_INSERT_ID()
        public int SaveGame(RawgGame rawgGame)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"INSERT INTO games (title, publisher, game_description, avg_rating, total_ratings)
                            VALUES (@title, @publisher, @description, @rating, @totalRatings);
                            SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@title", rawgGame.Title ?? "Unknown");
            cmd.Parameters.AddWithValue("@publisher", rawgGame.Publisher ?? "Unknown");
            cmd.Parameters.AddWithValue("@description", rawgGame.Description ?? "");
            cmd.Parameters.AddWithValue("@rating", rawgGame.Rating);
            cmd.Parameters.AddWithValue("@totalRatings", rawgGame.RatingsCount);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        // Resolves each genre name to an id, creating it if it doesn't exist yet
        public void SaveGameGenres(int gameId, List<string> genres)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "INSERT IGNORE INTO game_genres (game_id, genre_id) VALUES (@gameId, @genreId)";
            foreach (string genreName in genres)
            {
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.Parameters.AddWithValue("@genreId", _GetOrCreateGenre(genreName, connection));
                cmd.ExecuteNonQuery();
            }
        }

        // Batch save with progress logging and error counting
        public void SaveGames(List<RawgGame> games)
        {
            Console.WriteLine($"\nSaving {games.Count} games to database...");
            int saved = 0, errors = 0;

            foreach (var game in games)
            {
                try
                {
                    int gameId = SaveGame(game);
                    if (game.Genres != null && game.Genres.Count > 0) SaveGameGenres(gameId, game.Genres);
                    saved++;
                    if (saved % 100 == 0) Console.WriteLine($"Saved {saved}/{games.Count} games...");
                }
                catch (Exception ex)
                {
                    errors++;
                    Console.WriteLine($"Error saving '{game.Title}': {ex.Message}");
                }
            }

            Console.WriteLine($"\nDone. Saved: {saved}." + (errors > 0 ? $" Errors: {errors}." : ""));
        }

        #endregion

        #region Fetching

        public List<Game> GetAllGames()
        {
            // Return cached result if still fresh
            if (_cachedGames != null && DateTime.Now < _cacheExpiry) return _cachedGames;

            var games = new List<Game>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = $"{BaseGameQuery} {BaseGameQueryGroup}";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) games.Add(_MapGame(reader));

            _cachedGames = games;
            _cacheExpiry = DateTime.Now.AddMinutes(AppConfig.GameCacheMinutes);
            return _cachedGames;
        }

        public List<Game> GetGamesByGenres(List<int> genreIds)
        {
            var games = new List<Game>();
            using var connection = _pool.GetConnection();
            connection.Open();
            // Build parameterised IN clause — can't use a single param for a list
            string idParams = string.Join(",", genreIds.Select((_, i) => $"@genreId{i}"));
            string query = $@"{BaseGameQuery}
                                WHERE gg.genre_id IN ({idParams})
                                {BaseGameQueryGroup}";
            using var cmd = new MySqlCommand(query, connection);
            for (int i = 0; i < genreIds.Count; i++)
                cmd.Parameters.AddWithValue($"@genreId{i}", genreIds[i]);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) games.Add(_MapGame(reader));
            return games;
        }

        // Paginates by offset — page is 1-indexed
        public List<Game> GetTopGames(int page = 1, int pageSize = AppConfig.DefaultPageSize)
        {
            var games = new List<Game>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"SELECT id, title, publisher, game_description, avg_rating, total_ratings
                            FROM games
                            WHERE total_ratings > 0
                            ORDER BY avg_rating DESC, total_ratings DESC
                            LIMIT @pageSize OFFSET @offset";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@offset", (page - 1) * pageSize);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) games.Add(_MapGame(reader));
            return games;
        }

        public int GetTotalGameCount()
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM games";
            using var cmd = new MySqlCommand(query, connection);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public List<string> GetGameGenres(int gameId)
        {
            var genres = new List<string>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"SELECT g.genre_name
                            FROM genres g
                            INNER JOIN game_genres gg ON g.id = gg.genre_id
                            WHERE gg.game_id = @gameId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) genres.Add(reader.GetString("genre_name"));
            return genres;
        }

        #endregion

        #region Helpers

        // Looks up genre by name, inserts it if missing, returns the id either way
        private int _GetOrCreateGenre(string genreName, MySqlConnection connection)
        {
            string selectQuery = "SELECT id FROM genres WHERE genre_name = @genreName";
            using var selectCmd = new MySqlCommand(selectQuery, connection);
            selectCmd.Parameters.AddWithValue("@genreName", genreName);
            object result = selectCmd.ExecuteScalar();
            if (result != null) return Convert.ToInt32(result);

            string insertQuery = "INSERT INTO genres (genre_name) VALUES (@genreName); SELECT LAST_INSERT_ID();";
            using var insertCmd = new MySqlCommand(insertQuery, connection);
            insertCmd.Parameters.AddWithValue("@genreName", genreName);
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }

        private Game _MapGame(MySqlDataReader reader)
        {
            // genres comes back as a comma-separated string from GROUP_CONCAT
            string genreString = reader.IsDBNull(reader.GetOrdinal("genres")) ? "" : reader.GetString("genres");
            var genres = string.IsNullOrEmpty(genreString)
                ? new List<string>()
                : genreString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return new Game
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Publisher = reader.GetString("publisher"),
                GameDescription = reader.IsDBNull(reader.GetOrdinal("game_description")) ? "" : reader.GetString("game_description"),
                AvgRating = reader.IsDBNull(reader.GetOrdinal("avg_rating")) ? (decimal?)null : reader.GetDecimal("avg_rating"),
                TotalRatings = reader.IsDBNull(reader.GetOrdinal("total_ratings")) ? (int?)null : reader.GetInt32("total_ratings"),
                Genres = genres
            };
        }

        #endregion
    }
}