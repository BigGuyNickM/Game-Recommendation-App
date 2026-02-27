using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Game_Recommendation.Data;
using Game_Recommendation.Models;

namespace Game_Recommendation.Repositories
{
    public class GenreRepository
    {
        private readonly ConnectionPool _pool;

        public GenreRepository()
        {
            _pool = ConnectionPool.Instance;
        }

        // Gets all genres from the database
        public List<Genre> GetAllGenres()
        {
            List<Genre> genres = new List<Genre>();
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "SELECT id, genre_name FROM genres ORDER BY genre_name";

                MySqlCommand cmd = new MySqlCommand(query, connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        genres.Add(new Genre
                        {
                            Id = reader.GetInt32("id"),
                            GenreName = reader.GetString("genre_name")
                        });
                    }
                }
            }
            return genres;
        }

        // Saves the user's preferred genres to the database
        public void SaveUserPreferences(int userId, List<int> genreIds)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                foreach (int genreId in genreIds)
                {
                    string query = "INSERT INTO users_preferred_genres (user_id, genre_id) VALUES (@userId, @genreId) " +
                                   "ON DUPLICATE KEY UPDATE user_id = user_id";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@genreId", genreId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        // Checks if the user has any preferred genres saved
        public bool HasPreferences(int userId)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM users_preferred_genres WHERE user_id = @userId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // Get's the User's preferred genres from the database
        public List<Genre> GetUserPreferredGenres(int userId)
        {
            List<Genre> userGenres = new List<Genre>();

            using (var connection = _pool.GetConnection())
            {
                connection.Open();

                string query = @"SELECT g.id, g.genre_name 
                        FROM genres g
                        INNER JOIN users_preferred_genres upg ON g.id = upg.genre_id
                        WHERE upg.user_id = @userId
                        ORDER BY g.genre_name";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userId", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        userGenres.Add(new Genre
                        {
                            Id = reader.GetInt32("id"),
                            GenreName = reader.GetString("genre_name")
                        });
                    }
                }
            }
            return userGenres;
        }

        // Removes a specific genre from the user's preferences
        public void RemoveUserPreference(int userId, int genreId)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM users_preferred_genres WHERE user_id = @userId AND genre_id = @genreId";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@genreId", genreId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        // Adds a specific genre to the user's preferences
        public void AddUserPreference(int userId, int genreId)
        {
            using (var connection = _pool.GetConnection())
            {
                connection.Open();
                string query = "INSERT INTO users_preferred_genres (user_id, genre_id) VALUES (@userId, @genreId)";
                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@userId", userId);
                    cmd.Parameters.AddWithValue("@genreId", genreId);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
