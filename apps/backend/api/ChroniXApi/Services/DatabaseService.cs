using Microsoft.Data.Sqlite;

namespace ChroniXApi.Services
{
    public class DatabaseService
    {
        private const string ConnectionString = "Data Source=chronix.db";

        public void Initialize()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS ProcessTimes(
                    processName TEXT PRIMARY KEY,
                    seconds INTEGER NOT NULL
                )";
            command.ExecuteNonQuery();
        }

        public void SaveTime(string processName, int seconds)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO ProcessTimes(processName, seconds)
                VALUES($name, $seconds)";
            command.Parameters.AddWithValue("$name", processName);
            command.Parameters.AddWithValue("$seconds", seconds);
            command.ExecuteNonQuery();
        }

        public Dictionary<string, int> LoadTimes()
        {
            var result = new Dictionary<string, int>();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT processName, seconds FROM ProcessTimes";

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var processName = reader.GetString(0);
                var seconds = reader.GetInt32(1);

                result[processName] = seconds;
            }

            return result;
        }
    }
}
