using System;
using System.Configuration;
using MySql.Data.MySqlClient;

namespace Game_Recommendation.Data
{
    public class ConnectionPool
    {
        private static readonly Lazy<ConnectionPool> _instance = new Lazy<ConnectionPool>(() => new ConnectionPool());

        public static ConnectionPool Instance => _instance.Value;

        private readonly string _connectionString;

        private ConnectionPool()
        {
            _connectionString = ConfigurationManager
                .ConnectionStrings["GameStoreDB"].ConnectionString;
        }

        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(_connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}