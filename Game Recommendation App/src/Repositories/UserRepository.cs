using System;
using MySql.Data.MySqlClient;
using Game_Recommendation.Data;
using Game_Recommendation.Models;

namespace Game_Recommendation.Repositories
{
    public class UserRepository
    {
        private readonly ConnectionPool _pool;

        public UserRepository()
        {
            _pool = ConnectionPool.Instance;
        }

        public bool UsernameExists(string username)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users WHERE username = @username";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }
            }
        }

        // Get's a user by their username
        public User GetUserByUsername(string username)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "SELECT id, username, email, created_at FROM users WHERE username = @username";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                Email = reader.GetString("email"),
                                CreatedAt = reader.GetDateTime("created_at")
                            };
                        }
                    }
                }
            }
            return null;
        }

        // Creates a new user in the database and returns the created user object
        public User CreateUser(string username, string email)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                string query = "INSERT INTO users (username, email) VALUES (@username, @email); SELECT LAST_INSERT_ID();";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@email", email);

                    int newUserId = Convert.ToInt32(cmd.ExecuteScalar());

                    return new User
                    {
                        Id = newUserId,
                        Username = username,
                        Email = email,
                        CreatedAt = DateTime.Now
                    };
                }
            }
        }
    }
}
