using System.Configuration;
using MySql.Data.MySqlClient;

namespace Game_Recommendation.Data
{
    public class MySqlConnectionFactory
    {
        private readonly string connectionString;

        public MySqlConnectionFactory()
        {
            // Get connection string from App.config
            connectionString = ConfigurationManager.ConnectionStrings["GameStoreDB"].ConnectionString;
        }

        public MySqlConnection CreateConnection()
        {
            return new MySqlConnection(connectionString);
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = CreateConnection())
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