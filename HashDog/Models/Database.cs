using System;
using System.IO;
using System.Collections.Generic;
using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace HashDog
{
    public class Database : IDisposable
    {
        public SqliteConnection Connection; 
        public string TableName;
        public string TablePath;
        public bool IsTableLocked;

        public Database(string tablePath)
        {
            TablePath = tablePath;
            TableName = SanitizeTableName(TablePath);
            IsTableLocked = false;
            Connection = new SqliteConnection("Data Source=hashdog.db");

            Connection.Open();

            HandleLockTable();
        }

        private static string SanitizeTableName(string path)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            string sanitizedTableName = fileNameWithoutExtension.Replace(" ", "_");
            return sanitizedTableName;
        }

        private void HandleLockTable()
        {
            if (!LockTableExists())
            {
                CreateLockTable();
                InsertLockTablePath();
            }
            else
            {
                if (GetLockTablePaths().Contains(TablePath))
                {
                    IsTableLocked = true;
                }
                else
                {
                    InsertLockTablePath();
                }
            }    
        }

        private void CreateLockTable()
        {
            var command = Connection.CreateCommand();
            command.CommandText =
            $@"
                CREATE TABLE IF NOT EXISTS locked_dog (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    filepath TEXT
                );";
            command.ExecuteNonQuery();
        }

        private List<string> GetLockTablePaths()
        {
            List<string> lockedTablePaths = new List<string>();
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                SELECT filepath FROM locked_dog;
            ";
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    lockedTablePaths.Add(reader.GetString(0));
                }
            }
            return lockedTablePaths;
        }

        private void InsertLockTablePath()
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO locked_dog (filepath)
                VALUES (@filepath);
            ";
            command.Parameters.AddWithValue("@filepath", TablePath);
            command.ExecuteNonQuery();
        }

        public void RemoveLockTablePath()
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                DELETE FROM locked_dog
                WHERE filepath = @filepath;
            ";
            command.Parameters.AddWithValue("@filepath", TablePath);
            command.ExecuteNonQuery();
        }

        public bool IsFilePathExistInTable(string filePath)
        {
            var command = Connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM {TableName} WHERE filepath = @filePath";
            command.Parameters.AddWithValue("@filePath", filePath);

            int rowCount = Convert.ToInt32(command.ExecuteScalar());

            return rowCount > 0;
        }

        public bool IsTablePathExistInMetadataTable()
        {
            var command = Connection.CreateCommand();
            command.CommandText = $"SELECT COUNT(*) FROM hashdog_metadata WHERE tablepath = @tablepath";
            command.Parameters.AddWithValue("@tablepath", TablePath);

            int rowCount = Convert.ToInt32(command.ExecuteScalar());

            return rowCount > 0;
        }

        public bool HashDogTableExists()
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
            command.Parameters.AddWithValue("@tableName", TableName);

            int count = Convert.ToInt32(command.ExecuteScalar());

            return count > 0;
        }

        public bool LockTableExists()
        {
            var command = Connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@tableName";
            command.Parameters.AddWithValue("@tableName", "locked_dog");

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

                CREATE TABLE IF NOT EXISTS hashdog_metadata (
                    id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    tablepath TEXT,
                    hashtype TEXT,
                    table_created DATE,
                    run_frequency_minutes INTEGER,
                    run_schedule DATE,
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


        public void InsertMetadata(HashType hashType, int runFrequencyMinutes)
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                INSERT INTO hashdog_metadata (tablepath, hashtype, table_created, run_frequency_minutes, run_schedule)
                VALUES (@tablepath, @hashtype, @table_created, @run_frequency_minutes, @run_schedule);
            ";
            command.Parameters.AddWithValue("@tablepath", TablePath);
            command.Parameters.AddWithValue("@hashtype", Parser.ParseHashTypeToString(hashType));
            command.Parameters.AddWithValue("@run_frequency_minutes", runFrequencyMinutes);
            command.Parameters.AddWithValue("@run_schedule", DateTime.Now.Add(TimeSpan.FromMinutes(runFrequencyMinutes)));
            command.Parameters.AddWithValue("@table_created", DateTime.Now);
            command.ExecuteNonQuery();
        }

        public DateTime GetScheduledRun()
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                SELECT run_schedule FROM hashdog_metadata where tablepath=@tablepath;
            ";
            command.Parameters.AddWithValue("@tablepath", TablePath);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetDateTime(0);
                }
            }
            throw new Exception();
        }

        public int GetRunFrequencyMinutes()
        {
            var command = Connection.CreateCommand();
            command.CommandText = $@"
                SELECT run_frequency_minutes FROM hashdog_metadata where tablepath=@tablepath;
            ";
            command.Parameters.AddWithValue("@tablepath", TablePath);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetInt32(0);
                }
            }
            throw new Exception();
        }

        public void UpdateScheduleRun()
        {
            int runFrequencyMinutes = GetRunFrequencyMinutes();
            DateTime scheduleRunTime = GetScheduledRun();
            DateTime newScheduleRunTime = scheduleRunTime.AddMinutes(runFrequencyMinutes);

            var command = Connection.CreateCommand();
            command.CommandText = @"
                UPDATE hashdog_metadata 
                SET run_schedule = @nextScheduledRunTime
                WHERE tablepath = @tablepath;
            ";
            command.Parameters.AddWithValue("@nextScheduledRunTime", newScheduleRunTime);
            command.Parameters.AddWithValue("@tablepath", TablePath);
            command.ExecuteNonQuery();
        }

        public void UpdateScheduleRunSkipped()
        {
            var command = Connection.CreateCommand();
            command.CommandText = @"
                UPDATE hashdog_metadata 
                SET run_schedule = @nextScheduledRunTime
                WHERE tablepath = @tablepath;
            ";
            command.Parameters.AddWithValue("@nextScheduledRunTime", DateTime.Now);
            command.Parameters.AddWithValue("@tablepath", TablePath);
            command.ExecuteNonQuery();
        }

    

        public int InsertData(string filePath, string hashValue)
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

            command.CommandText = "SELECT last_insert_rowid()";
            return Convert.ToInt32(command.ExecuteScalar());
        }

        public void FirstRunArchiveCopy(int id)
        {
            string query = $@"
                SELECT * FROM {TableName}
                WHERE id=@id
            ";
            var command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@id", id);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    string hashValueBefore = reader.GetString(2);
                    DateTime timestampBefore = reader.GetDateTime(3);

                    var archiveHashValueTimestampBefore = Connection.CreateCommand();
                    archiveHashValueTimestampBefore.CommandText = $@"
                        INSERT INTO {TableName}_archive (hash_value_before, timestamp_before, {TableName}_id, result)
                        VALUES (@hash_value_before, @timestamp_before, @{TableName}_id, @result);
                    ";
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@hash_value_before", hashValueBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@timestamp_before", timestampBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue($"@{TableName}_id", id);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@result", Parser.ParseHashCompareResultToString(HashCompareResult.firstRun));
                    archiveHashValueTimestampBefore.ExecuteNonQuery();
                }
            }
        }

        public void FirstRunArchiveCopyByPath(string path)
        {
            string query = $@"
                SELECT * FROM {TableName}
                WHERE filepath=@filepath
            ";
            var command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@filepath", path);
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string hashValueBefore = reader.GetString(2);
                    DateTime timestampBefore = reader.GetDateTime(3);

                    var archiveHashValueTimestampBefore = Connection.CreateCommand();
                    archiveHashValueTimestampBefore.CommandText = $@"
                        INSERT INTO {TableName}_archive (hash_value_before, timestamp_before, {TableName}_id, result)
                        VALUES (@hash_value_before, @timestamp_before, @{TableName}_id, @result);
                    ";
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@hash_value_before", hashValueBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@timestamp_before", timestampBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue($"@{TableName}_id", id);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@result", Parser.ParseHashCompareResultToString(HashCompareResult.firstRun));
                    archiveHashValueTimestampBefore.ExecuteNonQuery();
                }
            }
        }


        public int SubsequentRunArchiveCopyBefore(int id)
        {
            string query = $@"SELECT id, hash_value, timestamp FROM {TableName} WHERE id=@id";
            var command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@id", id);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int hashDogId = reader.GetInt32(0);
                    string hashValueBefore = reader.GetString(1);
                    DateTime timestampBefore = reader.GetDateTime(2);

                    var archiveHashValueTimestampBefore = Connection.CreateCommand();
                    archiveHashValueTimestampBefore.CommandText = $@"
                        INSERT INTO {TableName}_archive (hash_value_before, timestamp_before, {TableName}_id)
                        VALUES (@hash_value_before, @timestamp_before, @{TableName}_id);
                    ";
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@hash_value_before", hashValueBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue("@timestamp_before", timestampBefore);
                    archiveHashValueTimestampBefore.Parameters.AddWithValue($"@{TableName}_id", hashDogId);
                    archiveHashValueTimestampBefore.ExecuteNonQuery();

                    var lastRowIdCommand = Connection.CreateCommand();
                    lastRowIdCommand.CommandText = "SELECT LAST_INSERT_ROWID()";
                    return Convert.ToInt32(lastRowIdCommand.ExecuteScalar());
                }
            }
            throw new Exception();
        }

        public void SubsequentRunArchiveCopyAfter(int mainId, int archiveId)
        {
            string query = $@"SELECT hash_value, timestamp FROM {TableName} WHERE id=@mainId";
            var commandSelect = new SqliteCommand(query, Connection);
            commandSelect.Parameters.AddWithValue("@mainId", mainId);

            using (var reader = commandSelect.ExecuteReader())
            {
                if (reader.Read())
                {
                    string hashValue = reader.GetString(0);
                    DateTime timestamp = reader.GetDateTime(1);

                    query = $@"
                        UPDATE {TableName}_archive
                        SET hash_value_after = @hashValueAfter, timestamp_after = @timestampAfter
                        WHERE id = @archiveId AND {TableName}_id = @mainId
                    ";

                    var commandUpdate = new SqliteCommand(query, Connection);
                    commandUpdate.Parameters.AddWithValue("@archiveId", archiveId);
                    commandUpdate.Parameters.AddWithValue("@mainId", mainId);
                    commandUpdate.Parameters.AddWithValue("@hashValueAfter", hashValue);
                    commandUpdate.Parameters.AddWithValue("@timestampAfter", timestamp);
                    commandUpdate.ExecuteNonQuery();
                }
            }
        }
        
        public void HandleArchiveComparisonResult(int archiveId)
        {
            string query = $@"
                SELECT hash_value_before, hash_value_after FROM {TableName}_archive
                WHERE id=@id            
                ;";
            var commandSelect = new SqliteCommand(query, Connection);
            commandSelect.Parameters.AddWithValue("@id", archiveId);

            using (var reader = commandSelect.ExecuteReader())
            {
                if (reader.Read())
                {
                    if (reader.IsDBNull(1))
                    {
                        UpdateArchiveComparisonResult(archiveId, HashCompareResult.firstRun);
                    }
                    else
                    {
                        string hashValueBefore = reader.GetString(0);
                        string hashValueAfter = reader.GetString(1);

                        if (string.Equals(hashValueBefore, hashValueAfter))
                        {
                            UpdateArchiveComparisonResult(archiveId, HashCompareResult.match);
                        }
                        else
                        {
                            UpdateArchiveComparisonResult(archiveId, HashCompareResult.mismatch);
                        }
                    }
                }
            }
        }

        private void UpdateArchiveComparisonResult(int archiveId, HashCompareResult hashCompareResult)
        {
            Console.WriteLine(archiveId);
            Console.WriteLine(Parser.ParseHashCompareResultToString(hashCompareResult));
            string query = $@"
                UPDATE {TableName}_archive
                SET result = @result
                WHERE id=@id            
                ;";
            var commandUpdate = new SqliteCommand(query, Connection);
            commandUpdate.Parameters.AddWithValue("@id", archiveId);
            commandUpdate.Parameters.AddWithValue("@result", Parser.ParseHashCompareResultToString(hashCompareResult));
            commandUpdate.ExecuteNonQuery();
        }

        public void UpdateData(int id, string hashValue)
        {
            Console.WriteLine("Updating data... table row has existing hashValue");

            var command = Connection.CreateCommand();
            command.CommandText = $@"
                UPDATE {TableName}
                SET hash_value = @hashValue, timestamp = @timestamp
                WHERE id = @id;
            ";
            command.Parameters.AddWithValue("@hashValue", hashValue);
            command.Parameters.AddWithValue("@timestamp", DateTime.Now);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public HashType GetTableHashType()
        {
            string query = $@"
                SELECT hashtype FROM hashdog_metadata 
                WHERE @tablepath=@tablepath;
            ";
            var command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@tablepath", TablePath);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    return Parser.ParseStringToHashType(reader.GetString(0));
                }
            }
            throw new Exception();
        }

        public List<int> GetHashDogTableIds()
        {
            List<int> ids = new List<int>();
            string query = $@"SELECT * FROM {TableName}";
            var command = new SqliteCommand(query, Connection);
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32(0));
                }
            }
            return ids;
        }

        public string GetHashDogTableFilepath(int id)
        {
            string query = $"SELECT filepath FROM {TableName} WHERE id=@id";
            var command = new SqliteCommand(query, Connection);
            command.Parameters.AddWithValue("@id", id);

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return reader.GetString(0); 
                }
            }
            throw new Exception();
        }
    }
}
