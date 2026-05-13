using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System.Data;
using Formatting = Newtonsoft.Json.Formatting;

namespace api.services.Handlers
{
    public class SqliteHandler
    {
        public static string ConnectionString = string.Empty;

        public static string GetJson(string request)
        {
            return JsonConvert.SerializeObject(GetDt(request), Formatting.Indented);
        }

        public static string GetScalar(string request)
        {
            string scalarResult = string.Empty;

            using SqliteConnection cnn = new(ConnectionString);
            cnn.Open();

            using SqliteCommand mycommand = new(request, cnn);
            object? result = mycommand.ExecuteScalar();

            if (result != null)
            {
                scalarResult = result.ToString() ?? string.Empty;
            }

            return scalarResult;
        }

        public static DataTable GetDt(string query)
        {
            DataTable dt = new();

            using SqliteConnection cnn = new(ConnectionString);
            cnn.Open();

            using SqliteCommand mycommand = new(query, cnn);
            using SqliteDataReader reader = mycommand.ExecuteReader();
            dt.Load(reader);

            return dt;
        }

        public static bool Exec(string query)
        {
            using SqliteConnection conn = new(ConnectionString);
            using SqliteCommand command = new(query, conn);

            try
            {
                conn.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
