// <copyright file="SqlServerFixture.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Sdk
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using Microsoft.SqlServer.Management.Common;
    using Microsoft.SqlServer.Management.Smo;

    public class SqlServerFixture : IDisposable
    {
        private readonly string databaseName = Guid.NewGuid().ToString("N");
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["SqlDatabase"].ConnectionString;

        public SqlServerFixture()
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var serverConnection = new ServerConnection(connection);
                var server = new Server(serverConnection);
                var database = new Database(server, this.databaseName);

                database.Create();
            }
        }

        public string ConnectionString
        {
            get { return this.connectionString; }
        }

        public string DatabaseName
        {
            get { return this.databaseName; }
        }

        public void Dispose()
        {
            using (var connection = new SqlConnection(this.connectionString))
            {
                var serverConnection = new ServerConnection(connection);
                var server = new Server(serverConnection);

                server.KillDatabase(this.databaseName);
            }
        }
    }
}
