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
            string serverConnectionString = ConfigurationManager.ConnectionStrings["ServerConnection"].ConnectionString;
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", "CreateDatabase.sql");

            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException("Не найден SQL-скрипт создания базы данных.", scriptPath);
            }

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

        private static List<string> SplitSqlScript(string scriptText)
        {
            string[] rawBatches = Regex.Split(scriptText, @"^\s*GO\s*($|\-\-.*$)", RegexOptions.Multiline | RegexOptions.IgnoreCase);
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
}
