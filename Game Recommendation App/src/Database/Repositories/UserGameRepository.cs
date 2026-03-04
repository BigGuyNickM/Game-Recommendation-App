using Game_Recommendation.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Game_Recommendation.Database.Repositories
{
    public class UserGameRepository
    {
        private readonly ConnectionPool _pool;

        public UserGameRepository(ConnectionPool pool)
        {
            _pool = pool;
        }

        #region Setup

        // Seeds the ratings table with discrete values if not already seeded
        public void SeedRatings()
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"INSERT IGNORE INTO ratings (rating_name, rating_value) VALUES
                            ('Disliked', 1),
                            ('Liked',    2),
                            ('Loved',    3)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region User Games

        public bool IsGamePlayed(int userId, int gameId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM users_games WHERE user_id = @userId AND game_id = @gameId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void AddPlayedGame(int userId, int gameId, int ratingId, int hoursPlayed)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"INSERT INTO users_games (user_id, game_id, rating_id, hours_played)
                            VALUES (@userId, @gameId, @ratingId, @hoursPlayed)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.Parameters.AddWithValue("@ratingId", ratingId);
            cmd.Parameters.AddWithValue("@hoursPlayed", hoursPlayed);
            cmd.ExecuteNonQuery();
        }

        public void RemovePlayedGame(int userId, int gameId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "DELETE FROM users_games WHERE user_id = @userId AND game_id = @gameId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.ExecuteNonQuery();
        }

        public List<int> GetRatings()
        {
            var ratings = new List<int>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT id FROM ratings ORDER BY rating_value";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) ratings.Add(reader.GetInt32("id"));
            return ratings;
        }

        public List<UserGame> GetPlayedGames(int userId)
        {
            var games = new List<UserGame>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"SELECT g.id, g.title, r.rating_name, ug.hours_played
                            FROM users_games ug
                            INNER JOIN games g ON ug.game_id = g.id
                            LEFT JOIN ratings r ON ug.rating_id = r.id
                            WHERE ug.user_id = @userId
                            ORDER BY ug.created_at DESC";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) games.Add(_MapUserGame(reader));
            return games;
        }

        #endregion

        #region Wishlist

        public bool IsGameWishlisted(int userId, int gameId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM users_wishlist WHERE user_id = @userId AND game_id = @gameId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public void AddToWishlist(int userId, int gameId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "INSERT IGNORE INTO users_wishlist (user_id, game_id) VALUES (@userId, @gameId)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.ExecuteNonQuery();
        }

        public void RemoveFromWishlist(int userId, int gameId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "DELETE FROM users_wishlist WHERE user_id = @userId AND game_id = @gameId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@gameId", gameId);
            cmd.ExecuteNonQuery();
        }

        public List<UserWishlist> GetWishlistedGames(int userId)
        {
            var games = new List<UserWishlist>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"SELECT g.id, g.title
                            FROM users_wishlist uw
                            INNER JOIN games g ON uw.game_id = g.id
                            WHERE uw.user_id = @userId
                            ORDER BY uw.created_at DESC";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) games.Add(_MapUserWishlist(reader));
            return games;
        }

        #endregion

        #region Helpers

        private UserGame _MapUserGame(MySqlDataReader reader) => new UserGame
        {
            Id = reader.GetInt32("id"),
            Title = reader.GetString("title"),
            RatingName = reader.IsDBNull(reader.GetOrdinal("rating_name")) ? null : reader.GetString("rating_name"),
            HoursPlayed = reader.GetInt32("hours_played")
        };

        private UserWishlist _MapUserWishlist(MySqlDataReader reader) => new UserWishlist
        {
            Id = reader.GetInt32("id"),
            Title = reader.GetString("title")
        };

        #endregion
    }
}