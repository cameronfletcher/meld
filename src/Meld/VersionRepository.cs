// <copyright file="VersionRepository.cs" company="Meld contributors">
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
                new SqlDatabase(connectionString, typeof(VersionRepository)).InitializeSchema(SchemaName);
            }

            this.connectionString = connectionString;
        }

        public int GetVersion(string databaseName, string schemaName)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "database.GetVersion";
                command.Parameters.Add("@Database", SqlDbType.VarChar, 511).Value = databaseName;
                command.Parameters.Add("@Schema", SqlDbType.VarChar, 128).Value = schemaName;

                connection.Open();

                using (var reader = command.ExecuteReader())
                {
                    reader.Read();
                    var result = reader["Version"];
                    return result == DBNull.Value ? 0 : Convert.ToInt32(result, CultureInfo.InvariantCulture);
                }
            }
        }

        public void SetVersion(string databaseName, string schemaName, int version, string description)
        {
            using (var connection = new SqlConnection(this.connectionString))
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "database.SetVersion";
                command.Parameters.Add("@Database", SqlDbType.VarChar, 511).Value = databaseName;
                command.Parameters.Add("@Schema", SqlDbType.VarChar, 128).Value = schemaName;
                command.Parameters.Add("@Version", SqlDbType.Int).Value = version;
                command.Parameters.Add("@Description", SqlDbType.VarChar).Value = description;

                connection.Open();

                command.ExecuteNonQuery();
            }
        }
    }
}
