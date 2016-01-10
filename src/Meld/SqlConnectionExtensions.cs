// <copyright file="SqlConnectionExtensions.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace System.Data.SqlClient
{
    using Meld;

    /// <summary>
    /// Provides extension methods for the <see cref="System.Data.SqlClient.SqlConnection"/> class.
    /// </summary>
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Initializes the database schema.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public static void InitializeSchema(this SqlConnection connection)
        {
            Guard.Against.Null(() => connection);
            Guard.Against.NullOrEmpty(() => connection.ConnectionString);

            new SqlDatabase(connection.ConnectionString).InitializeSchema();
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        public static void InitializeSchema(this SqlConnection connection, string schemaName)
        {
            Guard.Against.Null(() => connection);
            Guard.Against.NullOrEmpty(() => connection.ConnectionString);

            new SqlDatabase(connection.ConnectionString).InitializeSchema(schemaName);
        }

        /// <summary>
        /// Initializes the database schema for the specified type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type supported by the database schema.</param>
        public static void InitializeSchema(this SqlConnection connection, Type type)
        {
            Guard.Against.Null(() => connection);
            Guard.Against.NullOrEmpty(() => connection.ConnectionString);

            new SqlDatabase(connection.ConnectionString, type).InitializeSchema();
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name and type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="type">The type supported by the database schema.</param>
        public static void InitializeSchema(this SqlConnection connection, string schemaName, Type type)
        {
            Guard.Against.Null(() => connection);
            Guard.Against.NullOrEmpty(() => connection.ConnectionString);

            new SqlDatabase(connection.ConnectionString, type).InitializeSchema(schemaName);
        }
    }
}
