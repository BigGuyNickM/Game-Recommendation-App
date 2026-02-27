using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Game_Recommendation.Data;
using Game_Recommendation.Models;
using Game_Recommendation.Services;

namespace Game_Recommendation.Repositories
{
    public class GameRepository
    {
        private readonly ConnectionPool _pool;

        public GameRepository()
        {
            _pool = ConnectionPool.Instance;
        }

        // Save a single game to database
        public int SaveGame(RawgGame rawgGame)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                string query = @"INSERT INTO games (title, publisher, game_description, avg_rating, total_ratings) 
                                VALUES (@title, @publisher, @description, @rating, @totalRatings);
                                SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@title", rawgGame.Title ?? "Unknown");
                    cmd.Parameters.AddWithValue("@publisher", "Unknown");
                    cmd.Parameters.AddWithValue("@description", rawgGame.Description ?? "");
                    cmd.Parameters.AddWithValue("@rating", rawgGame.Rating);
                    cmd.Parameters.AddWithValue("@totalRatings", rawgGame.RatingsCount);

                    int gameId = Convert.ToInt32(cmd.ExecuteScalar());
                    return gameId;
                }
            }
        }

        // Save game genres (junction table)
        public void SaveGameGenres(int gameId, List<string> genres)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                foreach (string genreName in genres)
                {
                    int genreId = GetOrCreateGenre(genreName, connection);

                    string query = "INSERT IGNORE INTO game_genres (game_id, genre_id) VALUES (@gameId, @genreId)";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@gameId", gameId);
                        cmd.Parameters.AddWithValue("@genreId", genreId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private int GetOrCreateGenre(string genreName, MySqlConnection connection)
        {
            // Check if genre exists
            string selectQuery = "SELECT id FROM genres WHERE genre_name = @genreName";
            using (var cmd = new MySqlCommand(selectQuery, connection))
            {
                cmd.Parameters.AddWithValue("@genreName", genreName);
                object result = cmd.ExecuteScalar();

                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }

            // If genre doesn't exist we create it
            string insertQuery = "INSERT INTO genres (genre_name) VALUES (@genreName); SELECT LAST_INSERT_ID();";
            using (var cmd = new MySqlCommand(insertQuery, connection))
            {
                cmd.Parameters.AddWithValue("@genreName", genreName);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
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
                    // Save game and get its ID
                    int gameId = SaveGame(game);

                    // Save genres for this game
                    if (game.Genres != null && game.Genres.Count > 0)
                    {
                        SaveGameGenres(gameId, game.Genres);
                    }

                    saved++;

                    // Progress update every 100 games
                    if (saved % 100 == 0)
                    {
                        Console.WriteLine($"Saved {saved}/{games.Count} games...");
                    }
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
        public List<Game> GetTopGames(int page = 1, int pageSize = 10)
        {
            List<Game> games = new List<Game>();

            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                int offset = (page - 1) * pageSize;

                string query = @"SELECT id, title, publisher, game_description, avg_rating, total_ratings 
                        FROM games 
                        WHERE total_ratings > 0
                        ORDER BY avg_rating DESC, total_ratings DESC 
                        LIMIT @pageSize OFFSET @offset";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@pageSize", pageSize);
                    cmd.Parameters.AddWithValue("@offset", offset);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            games.Add(new Game
                            {
                                Id = reader.GetInt32("id"),
                                Title = reader.GetString("title"),
                                Publisher = reader.GetString("publisher"),
                                GameDescription = reader.IsDBNull(reader.GetOrdinal("game_description"))
                                    ? "" : reader.GetString("game_description"),
                                AvgRating = reader.IsDBNull(reader.GetOrdinal("avg_rating"))
                                    ? null : reader.GetDecimal("avg_rating"),
                                TotalRatings = reader.IsDBNull(reader.GetOrdinal("total_ratings"))
                                    ? null : reader.GetInt32("total_ratings")
                            });
                        }
                    }
                }
            }

            return games;
        }

        public int GetTotalGameCount()
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM games";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        public List<string> GetGameGenres(int gameId)
        {
            List<string> genres = new List<string>();

            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                string query = @"SELECT g.genre_name 
                        FROM genres g
                        INNER JOIN game_genres gg ON g.id = gg.genre_id
                        WHERE gg.game_id = @gameId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@gameId", gameId);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            genres.Add(reader.GetString("genre_name"));
                        }
                    }
                }
            }

            return genres;
        }
    }
}