﻿// <copyright file="VersionRepository.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;

    internal class VersionRepository : IVersionRepository
    {
        internal const string SchemaName = "database";

        private static readonly ConcurrentSet<string> InitializedDatabases = new ConcurrentSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly string connectionString;

        public VersionRepository(string connectionString)
        {
            Guard.Against.Null(() => connectionString);

            // NOTE (Cameron): This is *not* an example of how to use Meld.
            if (InitializedDatabases.TryAdd(connectionString))
            {
                new SqlDatabase(connectionString, "Meld").InitializeSchema(SchemaName);
            }

            this.connectionString = connectionString;
        }

        public Version GetVersion(string databaseName, string schemaName)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "database.GetVersion";
                command.Parameters.Add("@Database", SqlDbType.VarChar, 511).Value = databaseName;
                command.Parameters.Add("@Schema", SqlDbType.VarChar, 128).Value = schemaName;

                connection.Open();

                // NOTE (Cameron): This is designed to throw a SqlException on the first execution against a database only.
                using (var reader = command.ExecuteReader())
                {
                    var sqlBatches = new Dictionary<int, string>();

                    while (reader.Read())
                    {
                        var version = reader.GetInt32(0);
                        var script = reader.IsDBNull(1) ? null : reader.GetString(1);

                        sqlBatches.Add(version, script);
                    }

                    return new Version(sqlBatches);
                }
            }
        }

        public void SetVersion(string databaseName, string schemaName, string description, Version version)
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                var versionsData = new DataTable();
                versionsData.Columns.Add("Version").DataType = typeof(int);
                versionsData.Columns.Add("Script").DataType = typeof(string);

                foreach (var script in version.GetSqlScripts())
                {
                    versionsData.Rows.Add(
                        script.Version,
                        string.Concat(
                            string.Join("\r\nGO\r\n\r\n", script.GetSqlBatches(databaseName, schemaName, connection.ServerVersion)),
                            "\r\nGO"));
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.Text;
                    command.CommandText = @"CREATE TABLE #Versions (
    [Version] INT NOT NULL,
    [Script] VARCHAR(MAX) NOT NULL
);";

                    command.ExecuteNonQuery();
                }

                using (var bulkCopy = new SqlBulkCopy(connection))
                {
                    bulkCopy.DestinationTableName = "#Versions";
                    bulkCopy.WriteToServer(versionsData);
                }

                using (var command = connection.CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "database.SetVersion";
                    command.Parameters.Add("@Database", SqlDbType.VarChar, 511).Value = databaseName;
                    command.Parameters.Add("@Schema", SqlDbType.VarChar, 128).Value = schemaName;
                    command.Parameters.Add("@Description", SqlDbType.VarChar, -1).Value = description;

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
