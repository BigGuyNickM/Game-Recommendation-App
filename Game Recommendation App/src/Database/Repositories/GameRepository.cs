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

        // Save a single game to database
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

        // Save game genres (junction table)
        public void SaveGameGenres(int gameId, List<string> genres)
        {
            using var connection = _pool.GetConnection();
            connection.Open();

            foreach (string genreName in genres)
            {
                int genreId = GetOrCreateGenre(genreName, connection);

                string query = "INSERT IGNORE INTO game_genres (game_id, genre_id) VALUES (@gameId, @genreId)";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.Parameters.AddWithValue("@genreId", genreId);
                cmd.ExecuteNonQuery();
            }
        }

        private int GetOrCreateGenre(string genreName, MySqlConnection connection)
        {
            string selectQuery = "SELECT id FROM genres WHERE genre_name = @genreName";
            using var selectCmd = new MySqlCommand(selectQuery, connection);
            selectCmd.Parameters.AddWithValue("@genreName", genreName);
            object result = selectCmd.ExecuteScalar();

            if (result != null)
                return Convert.ToInt32(result);

            string insertQuery = "INSERT INTO genres (genre_name) VALUES (@genreName); SELECT LAST_INSERT_ID();";
            using var insertCmd = new MySqlCommand(insertQuery, connection);
            insertCmd.Parameters.AddWithValue("@genreName", genreName);
            return Convert.ToInt32(insertCmd.ExecuteScalar());
        }

        // Save multiple games (batch operation) for API services
        public void SaveGames(List<RawgGame> games)
        {
            Console.WriteLine($"\nSaving {games.Count} games to database...");

            int saved = 0;
            int errors = 0;

            foreach (var game in games)
            {
                try
                {
                    int gameId = SaveGame(game);

                    if (game.Genres != null && game.Genres.Count > 0)
                        SaveGameGenres(gameId, game.Genres);

                    saved++;

                    if (saved % 100 == 0)
                        Console.WriteLine($"Saved {saved}/{games.Count} games...");
                }
                catch (Exception ex)
                {
                    errors++;
                    Console.WriteLine($"Error saving game '{game.Title}': {ex.Message}");
                }
            }

            Console.WriteLine("\nDatabase save complete.");
            Console.WriteLine($"Successfully saved: {saved} games.");
            if (errors > 0)
                Console.WriteLine($"Errors: {errors} games.");
        }

        // Get top games by rating with pagination
        public List<Game> GetTopGames(int page = 1, int pageSize = AppConfig.DefaultPageSize)
        {
            List<Game> games = new();
            using var connection = _pool.GetConnection();
            connection.Open();

            int offset = (page - 1) * pageSize;
            string query = @"SELECT id, title, publisher, game_description, avg_rating, total_ratings 
                    FROM games 
                    WHERE total_ratings > 0
                    ORDER BY avg_rating DESC, total_ratings DESC 
                    LIMIT @pageSize OFFSET @offset";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@pageSize", pageSize);
            cmd.Parameters.AddWithValue("@offset", offset);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                games.Add(_MapGame(reader));

            return games;
        }

        public List<Game> GetGamesByGenres(List<int> genreIds)
        {
            List<Game> games = new();
            using var connection = _pool.GetConnection();
            connection.Open();

            string idParams = string.Join(",", genreIds.Select((_, i) => $"@genreId{i}"));
            string query = $@"{BaseGameQuery}
                    WHERE gg.genre_id IN ({idParams})
                    {BaseGameQueryGroup}";

            using var cmd = new MySqlCommand(query, connection);
            for (int i = 0; i < genreIds.Count; i++)
                cmd.Parameters.AddWithValue($"@genreId{i}", genreIds[i]);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                games.Add(_MapGame(reader));

            return games;
        }

        // Get all games from the database
        public List<Game> GetAllGames()
        {
            if (_cachedGames != null && DateTime.Now < _cacheExpiry)
                return _cachedGames;

            List<Game> games = new();
            using var connection = _pool.GetConnection();
            connection.Open();

            string query = $"{BaseGameQuery} {BaseGameQueryGroup}";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                games.Add(_MapGame(reader));

            _cachedGames = games;
            _cacheExpiry = DateTime.Now.AddMinutes(AppConfig.GameCacheMinutes);
            return _cachedGames;
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
            List<string> genres = new();

            using var connection = _pool.GetConnection();
            connection.Open();

            string query = @"SELECT g.genre_name 
                    FROM genres g
                    INNER JOIN game_genres gg ON g.id = gg.genre_id
                    WHERE gg.game_id = @gameId";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@gameId", gameId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                genres.Add(reader.GetString("genre_name"));

            return genres;
        }

        // --- Private helpers ---

        private Game _MapGame(MySqlDataReader reader)
        {
            string genreString = reader.IsDBNull(reader.GetOrdinal("genres")) ? "" : reader.GetString("genres");
            List<string> genres = string.IsNullOrEmpty(genreString)
                ? new List<string>()
                : genreString.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            return new Game
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Publisher = reader.GetString("publisher"),
                GameDescription = reader.IsDBNull(reader.GetOrdinal("game_description")) ? "" : reader.GetString("game_description"),
                AvgRating = reader.IsDBNull(reader.GetOrdinal("avg_rating")) ? null : reader.GetDecimal("avg_rating"),
                TotalRatings = reader.IsDBNull(reader.GetOrdinal("total_ratings")) ? null : reader.GetInt32("total_ratings"),
                Genres = genres
            };
        }
    }
}