using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Game_Recommendation.Models;

namespace Game_Recommendation.Database.Repositories
{
    public class GenreRepository
    {
        private readonly ConnectionPool _pool;

        public GenreRepository(ConnectionPool pool)
        {
            _pool = pool;
        }

        #region Public

        public List<Genre> GetAllGenres()
        {
            var genres = new List<Genre>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT id, genre_name FROM genres ORDER BY genre_name";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) genres.Add(_MapGenre(reader));
            return genres;
        }

        // ON DUPLICATE KEY skips silently if the genre is already saved
        public void SaveUserPreferences(int userId, List<int> genreIds)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "INSERT INTO users_preferred_genres (user_id, genre_id) VALUES (@userId, @genreId) " +
                           "ON DUPLICATE KEY UPDATE user_id = user_id";
            foreach (int genreId in genreIds)
            {
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@genreId", genreId);
                cmd.ExecuteNonQuery();
            }
        }

        public bool HasPreferences(int userId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "SELECT COUNT(*) FROM users_preferred_genres WHERE user_id = @userId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
        }

        public List<Genre> GetUserPreferredGenres(int userId)
        {
            var genres = new List<Genre>();
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = @"SELECT g.id, g.genre_name
                            FROM genres g
                            INNER JOIN users_preferred_genres upg ON g.id = upg.genre_id
                            WHERE upg.user_id = @userId
                            ORDER BY g.genre_name";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) genres.Add(_MapGenre(reader));
            return genres;
        }

        public void RemoveUserPreference(int userId, int genreId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "DELETE FROM users_preferred_genres WHERE user_id = @userId AND genre_id = @genreId";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@genreId", genreId);
            cmd.ExecuteNonQuery();
        }

        public void AddUserPreference(int userId, int genreId)
        {
            using var connection = _pool.GetConnection();
            connection.Open();
            string query = "INSERT INTO users_preferred_genres (user_id, genre_id) VALUES (@userId, @genreId)";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@genreId", genreId);
            cmd.ExecuteNonQuery();
        }

        #endregion

        #region Helpers

        private Genre _MapGenre(MySqlDataReader reader) => new Genre
        {
            Id = reader.GetInt32("id"),
            GenreName = reader.GetString("genre_name")
        };

        #endregion
    }
}