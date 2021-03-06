﻿// <copyright file="SqlConnectionExtensions.cs" company="Meld contributors">
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

            new SqlDatabase(connection.ConnectionString, type).InitializeSchema(schemaName);
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name and database name.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="databaseName">Name of the database.</param>
        public static void InitializeSchema(this SqlConnection connection, string schemaName, string databaseName)
        {
            Guard.Against.Null(() => connection);

            new SqlDatabase(connection.ConnectionString, databaseName).InitializeSchema(schemaName);
        }

        /// <summary>
        /// Initializes the database schema.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <returns>The initialized connection.</returns>
        public static SqlConnection WithInitializedSchema(this SqlConnection connection)
        {
            connection.InitializeSchema();
            return connection;
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>The initialized connection.</returns>
        public static SqlConnection WithInitializedSchema(this SqlConnection connection, string schemaName)
        {
            connection.InitializeSchema(schemaName);
            return connection;
        }

        /// <summary>
        /// Initializes the database schema for the specified type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="type">The type supported by the database schema.</param>
        /// <returns>The initialized connection.</returns>
        public static SqlConnection WithInitializedSchema(this SqlConnection connection, Type type)
        {
            connection.InitializeSchema(type);
            return connection;
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name and type.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="type">The type.</param>
        /// <returns>The initialized connection.</returns>
        public static SqlConnection WithInitializedSchema(this SqlConnection connection, string schemaName, Type type)
        {
            connection.InitializeSchema(schemaName, type);
            return connection;
        }

        /// <summary>
        /// Initializes the database schema for the specified schema name and database name.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The initialized connection.</returns>
        public static SqlConnection WithInitializedSchema(this SqlConnection connection, string schemaName, string databaseName)
        {
            connection.InitializeSchema(schemaName, databaseName);
            return connection;
        }
    }
}
