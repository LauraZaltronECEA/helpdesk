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

        public static string GetJson(string request, params SqliteParameter[] parameters)
        {
            return JsonConvert.SerializeObject(GetDt(request, parameters), Formatting.Indented);
        }

        public static string GetScalar(string request)
        {
            return GetScalar(request, Array.Empty<SqliteParameter>());
        }

        public static string GetScalar(string request, params SqliteParameter[] parameters)
        {
            string scalarResult = string.Empty;

            using SqliteConnection cnn = new(ConnectionString);
            cnn.Open();

            using SqliteCommand mycommand = new(request, cnn);
            AddParameters(mycommand, parameters);
            object? result = mycommand.ExecuteScalar();

            if (result != null)
            {
                scalarResult = result.ToString() ?? string.Empty;
            }

            return scalarResult;
        }

        public static DataTable GetDt(string query)
        {
            return GetDt(query, Array.Empty<SqliteParameter>());
        }

        public static DataTable GetDt(string query, params SqliteParameter[] parameters)
        {
            DataTable dt = new();

            using SqliteConnection cnn = new(ConnectionString);
            cnn.Open();

            using SqliteCommand mycommand = new(query, cnn);
            AddParameters(mycommand, parameters);
            using SqliteDataReader reader = mycommand.ExecuteReader();
            dt.Load(reader);

            return dt;
        }

        public static bool Exec(string query)
        {
            return Exec(query, Array.Empty<SqliteParameter>());
        }

        public static bool Exec(string query, params SqliteParameter[] parameters)
        {
            return ExecRows(query, parameters) > 0;
        }

        public static int ExecRows(string query, params SqliteParameter[] parameters)
        {
            using SqliteConnection conn = new(ConnectionString);
            using SqliteCommand command = new(query, conn);
            AddParameters(command, parameters);

            try
            {
                conn.Open();
                return command.ExecuteNonQuery();
            }
            catch
            {
                return 0;
            }
        }

        private static void AddParameters(SqliteCommand command, SqliteParameter[] parameters)
        {
            if (parameters.Length > 0)
            {
                command.Parameters.AddRange(parameters);
            }
        }
    }
}
