// <copyright file="SqlServerFeature.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Sdk
{
    using System.Data.SqlClient;
    using Microsoft.SqlServer.Management.Common;

    public abstract class SqlServerFeature
    {
        // LINK (Cameron): https://github.com/xbehave/xbehave.net/wiki/Can%27t-access-fixture-data-when-using-IUseFixture
        private Integration.Database integrationDatabase;

        public SqlServerFeature(SqlServerFixture fixture)
        {
            this.integrationDatabase = new Integration.Database(fixture);
        }

        public string ConnectionString
        {
            get { return this.integrationDatabase.ConnectionString; }
        }

        protected void ExecuteSql(string sql)
        {
            using (var connection = new SqlConnection(this.ConnectionString))
            {
                var serverConnection = new ServerConnection(connection);

                serverConnection.ExecuteNonQuery(sql);
            }
        }
    }
}
