using System;
using MySql.Data.MySqlClient;
using Game_Recommendation.Models;

namespace Game_Recommendation.Database.Repositories
{
    public class UserRepository
    {
        private readonly ConnectionPool _pool;

        public UserRepository(ConnectionPool pool)
        {
            _pool = pool;
        }

        #region Public

        public bool UsernameExists(string username)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM users WHERE username = @username";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public User GetUserByUsername(string username)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT id, username, email, created_at FROM users WHERE username = @username";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            using var reader = cmd.ExecuteReader();
            return reader.Read() ? _MapUser(reader) : null;
        }

        // INSERT then immediately grab the new row's id via LAST_INSERT_ID()
        public User CreateUser(string username, string email, string passwordHash)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "INSERT INTO users (username, email, password_hash) VALUES (@username, @email, @passwordHash); SELECT LAST_INSERT_ID();";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@passwordHash", passwordHash);
            return new User
            {
                Id = Convert.ToInt32(cmd.ExecuteScalar()),
                Username = username,
                Email = email,
                CreatedAt = DateTime.Now
            };
        }

        // Fetches just the hash — we don't want to pull the full user object before verifying
        public string GetPasswordHash(string username)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT password_hash FROM users WHERE username = @username";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@username", username);
            return cmd.ExecuteScalar()?.ToString();
        }

        #endregion

        #region Helpers

        private User _MapUser(MySqlDataReader reader) => new User
        {
            Id = reader.GetInt32("id"),
            Username = reader.GetString("username"),
            Email = reader.GetString("email"),
            CreatedAt = reader.GetDateTime("created_at")
        };

        #endregion
    }
}