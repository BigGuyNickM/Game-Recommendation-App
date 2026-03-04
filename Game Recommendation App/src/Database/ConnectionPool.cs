using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Game_Recommendation.Database
{
    public class ConnectionPool
    {
        private static readonly Lazy<ConnectionPool> _instance = new(() => new ConnectionPool());
        public static ConnectionPool Instance => _instance.Value;

        private readonly string _connectionString;

        private ConnectionPool()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["GameStoreDB"].ConnectionString;
        }

        public MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

        public bool TestConnection()
        {
            try
            {
                using var connection = GetConnection();
                connection.Open();
                return true;
            }
            catch { return false; }
        }
    }
}