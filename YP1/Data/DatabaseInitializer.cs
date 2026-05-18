using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace YP1.Data
{
    public class DatabaseInitializer
    {
        public void Initialize()
        {
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "CreateDatabase.sql");

            if (!File.Exists(scriptPath))
            {
                EnsureExistingDatabaseIsAvailable();
                return;
            }

            string serverConnectionString = ConnectionStringResolver.GetServerConnectionString();
            string scriptText = File.ReadAllText(scriptPath);
            List<string> batches = SplitSqlScript(scriptText);

            using (SqlConnection connection = new SqlConnection(serverConnectionString))
            {
                connection.Open();

                foreach (string batch in batches)
                {
                    using (SqlCommand command = new SqlCommand(batch, connection))
                    {
                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private static void EnsureExistingDatabaseIsAvailable()
        {
            string connectionString = ConnectionStringResolver.GetLibraryConnectionString();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(
                    @"SELECT COUNT(1)
                      FROM INFORMATION_SCHEMA.TABLES
                      WHERE TABLE_SCHEMA = 'dbo'
                        AND TABLE_NAME IN ('Users', 'Books', 'Reviews');",
                    connection))
                {
                    int tablesCount = Convert.ToInt32(command.ExecuteScalar());

                    if (tablesCount < 3)
                    {
                        throw new InvalidOperationException(
                            "Подключение к базе данных есть, но в ней не найдены нужные таблицы приложения.");
                    }
                }
            }
        }

        private static List<string> SplitSqlScript(string scriptText)
        {
            string[] rawBatches = Regex.Split(
                scriptText,
                @"^\s*GO\s*($|\-\-.*$)",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            List<string> result = new List<string>();

            foreach (string rawBatch in rawBatches)
            {
                string batch = rawBatch == null ? string.Empty : rawBatch.Trim();

                if (!string.IsNullOrWhiteSpace(batch))
                {
                    result.Add(batch);
                }
            }

            return result;
        }
    }

    public static class ConnectionStringResolver
    {
        public static string GetLibraryConnectionString()
        {
            List<string> candidates = new List<string>();

            string libraryConnection = ReadConnectionString("LibraryConnection");

            if (!string.IsNullOrWhiteSpace(libraryConnection))
            {
                candidates.Add(libraryConnection);
            }

            string entityConnection = ReadEntityProviderConnectionString("ReadWriteDontCheatDbEntities");

            if (!string.IsNullOrWhiteSpace(entityConnection))
            {
                bool alreadyAdded = false;

                foreach (string candidate in candidates)
                {
                    if (string.Equals(candidate, entityConnection, StringComparison.OrdinalIgnoreCase))
                    {
                        alreadyAdded = true;
                        break;
                    }
                }

                if (!alreadyAdded)
                {
                    candidates.Add(entityConnection);
                }
            }

            Exception lastException = null;

            foreach (string candidate in candidates)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(candidate))
                    {
                        connection.Open();
                        return candidate;
                    }
                }
                catch (Exception exception)
                {
                    lastException = exception;
                }
            }

            if (lastException != null)
            {
                throw new InvalidOperationException(
                    "Не удалось подключиться к базе данных ни по одной строке подключения.",
                    lastException);
            }

            throw new InvalidOperationException("В App.config не найдена строка подключения к базе данных.");
        }

        public static string GetServerConnectionString()
        {
            string serverConnection = ReadConnectionString("ServerConnection");

            if (!string.IsNullOrWhiteSpace(serverConnection))
            {
                return serverConnection;
            }

            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(GetLibraryConnectionString());
            builder.InitialCatalog = string.Empty;
            builder.AttachDBFilename = string.Empty;
            return builder.ConnectionString;
        }

        private static string ReadConnectionString(string name)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];
            return settings == null ? string.Empty : settings.ConnectionString;
        }

        private static string ReadEntityProviderConnectionString(string name)
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings[name];

            if (settings == null || string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                return string.Empty;
            }

            const string marker = "provider connection string=";
            string connectionString = settings.ConnectionString;
            int markerIndex = connectionString.IndexOf(marker, StringComparison.OrdinalIgnoreCase);

            if (markerIndex < 0)
            {
                return string.Empty;
            }

            string tail = connectionString.Substring(markerIndex + marker.Length).Trim();

            if (tail.StartsWith("\"", StringComparison.Ordinal) &&
                tail.EndsWith("\"", StringComparison.Ordinal) &&
                tail.Length >= 2)
            {
                return tail.Substring(1, tail.Length - 2);
            }

            return tail;
        }
    }
}
