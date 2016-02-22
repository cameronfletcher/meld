// <copyright file="SqlDatabase.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;

    internal class SqlDatabase
    {
        private static readonly ConcurrentDictionary<Key, Database> DatabaseRegistry = new ConcurrentDictionary<Key, Database>(new Key.Comparer());

        private readonly string name;

        public SqlDatabase(string connectionString)
            : this(connectionString, typeof(SqlDatabase))
        {
        }

        public SqlDatabase(string connectionString, Type type)
            : this(connectionString, GetName(type))
        {
        }

        public SqlDatabase(string connectionString, string name)
        {
            Guard.Against.NullOrEmpty(() => connectionString);
            Guard.Against.NullOrEmpty(() => name);

            this.ConnectionString = connectionString;
            this.name = name;
        }

        public string ConnectionString { get; private set; }

        public void InitializeSchema()
        {
            this.InitializeSchema("dbo");
        }

        public void InitializeSchema(string schemaName)
        {
            Guard.Against.NullOrEmpty(() => schemaName);

            var database = DatabaseRegistry.GetOrAdd(
                new Key { DatabaseName = this.name, SchemaName = schemaName },
                key => new Database { Schema = new SqlDatabaseSchema(key.DatabaseName, key.SchemaName) });

            if (database.ConnectionStrings.Contains(this.ConnectionString))
            {
                return;
            }

            database.Schema.Initialize(this);

            database.ConnectionStrings.Add(this.ConnectionString);
        }

        // LINK (Cameron): http://stackoverflow.com/questions/17008902/sending-several-sql-commands-in-a-single-transaction
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", Justification = "No vulnerability.")]
        public void Execute(IEnumerable<SqlScript> sqlScripts)
        {
            Guard.Against.NullOrEmptyOrNullElements(() => sqlScripts);

            using (var connection = new SqlConnection(this.ConnectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var sqlScript in sqlScripts)
                        {
                            foreach (var batch in sqlScript.SqlBatches)
                            {
                                using (var command = new SqlCommand(batch, connection, transaction))
                                {
                                    // NOTE (Cameron): Wow, that indented quickly.
                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        internal static string GetName(Type type)
        {
            Guard.Against.Null(() => type);

            const string repositorySuffix = "Repository";

            var typeName = type.Name;
            if (typeName.EndsWith(repositorySuffix, StringComparison.OrdinalIgnoreCase) &&
                typeName.Length > repositorySuffix.Length)
            {
                typeName = typeName.Substring(0, typeName.Length - repositorySuffix.Length);
            }

            return typeName;
        }

        private class Key
        {
            public string DatabaseName { get; set; }

            public string SchemaName { get; set; }

            public class Comparer : IEqualityComparer<Key>
            {
                [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "It's private.")]
                [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "It's private.")]
                public bool Equals(Key x, Key y)
                {
                    return string.Equals(x.DatabaseName, y.DatabaseName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(x.SchemaName, y.SchemaName, StringComparison.OrdinalIgnoreCase);
                }

                // LINK (Cameron): http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
                [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "It's private.")]
                public int GetHashCode(Key obj)
                {
                    unchecked
                    {
                        int hash = 17;
                        hash = (hash * 23) + obj.DatabaseName.GetHashCode();
                        hash = (hash * 23) + obj.SchemaName.GetHashCode();
                        return hash;
                    }
                }
            }
        }

        private class Database
        {
            private readonly List<string> connectionStrings = new List<string>();

            public List<string> ConnectionStrings
            {
                get { return this.connectionStrings; }
            }

            public SqlDatabaseSchema Schema { get; set; }
        }
    }
}
