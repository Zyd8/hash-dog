using System;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace HashDog
{
    public class Database : IDisposable
    {
        public SqliteConnection Connection; 
        public string TableName;

        public Database(string tableName)
        {
            TableName = tableName;
            Connection = new SqliteConnection("Data Source=hashdog.db");
            Connection.Open();
        }

        public bool IsFilePathExistInTable(string filePath)
        {
            var command = Connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {TableName} WHERE filepath = @filePath";
            command.Parameters.AddWithValue("@filePath", filePath);

            int rowCount = Convert.ToInt32(command.ExecuteScalar());

            return rowCount > 0;
        }

        public bool DoesTableExist()
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
            command.Parameters.AddWithValue("@tableName", TableName);

            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
            
        }

        public void CreateHashDog()
        {
            var command = Connection.CreateCommand();
            command.CommandText =
            $@"
                CREATE TABLE IF NOT EXISTS {TableName} (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    filepath TEXT,
                    hash_value TEXT,
                    timestamp DATE
                );

                CREATE TABLE IF NOT EXISTS {TableName}_metadata (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    hashtype TEXT,
                    table_created DATE
                );

                CREATE TABLE IF NOT EXISTS {TableName}_archive (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    hash_value_before TEXT,
                    timestamp_before DATE,
                    hash_value_after TEXT,                  
                    timestamp_after DATE,
                    result TEXT,
                    {TableName}_id INTEGER,
                    FOREIGN KEY ({TableName}_id) REFERENCES {TableName}(id) 
                );
            ";
            command.ExecuteNonQuery();
        }


        public void InsertMetadata(HashType hashType)
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO {TableName}_metadata (hashtype, table_created)
                VALUES (@hashtype, @table_created);
            ";
            command.Parameters.AddWithValue("@hashtype", hashType);
            command.Parameters.AddWithValue("@table_created", DateTime.Now);
            command.ExecuteNonQuery();
        }

        public void InsertData(string filePath, string hashValue)
        {
            Console.WriteLine("Inserting data... table row has no filePath yet");

            var command = Connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO {TableName} (filepath, hash_value, timestamp)
                VALUES (@filepath, @hashValue, @timestamp);
            ";
            command.Parameters.AddWithValue("@filepath", filePath);
            command.Parameters.AddWithValue("@hashValue", hashValue);
            command.Parameters.AddWithValue("@timestamp", DateTime.Now);
            command.ExecuteNonQuery();
        }

        public void FirstRunArchiveCopy()
        {
            string query = $@"SELECT * FROM {TableName}";
            var command = new SqliteCommand(query, Connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string hashValueAfter = reader.GetString(2);
                    DateTime timestampAfter = reader.GetDateTime(3);

                    var archiveHashValueTimestampAfter = Connection.CreateCommand();
                    archiveHashValueTimestampAfter.CommandText = $@"
                        INSERT INTO {TableName}_archive (hash_value_after, timestamp_after, {TableName}_id, result)
                        VALUES (@hash_value_after, @timestamp_after, @{TableName}_id, @result);
                    ";
                    archiveHashValueTimestampAfter.Parameters.AddWithValue("@hash_value_after", hashValueAfter);
                    archiveHashValueTimestampAfter.Parameters.AddWithValue("@timestamp_after", timestampAfter);
                    archiveHashValueTimestampAfter.Parameters.AddWithValue($"@{TableName}_id", id);
                    archiveHashValueTimestampAfter.Parameters.AddWithValue("@result", "First Run");
                    archiveHashValueTimestampAfter.ExecuteNonQuery();
                }
            }
        }

        public void UpdateData()
        {
            Console.WriteLine("Updating data... table row has existing filePath");
        }


        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
