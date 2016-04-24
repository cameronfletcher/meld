// <copyright file="SqlServerDatabase.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Feature
{
    using System.Data;
    using System.Data.SqlClient;
    using Sdk;
    using Xbehave;
    using Xunit;
    using FluentAssertions;
    using System;
    [Collection("SQL Server Collection")]
    public class SqlServerDatabase : SqlServerFeature
    {
        public SqlServerDatabase(SqlServerFixture fixture)
            : base(fixture)
        {
        }

        // can create database
        // can create multiple databases
        // rollback on error is atomic
        [Scenario]
        public void CanInitializeDatabase(SqlConnection connection, int result)
        {
            "Given a SQL Server connection"
                .f(c => connection = new SqlConnection(this.ConnectionString).Using(c));

            "When I initialize that connection for this test"
                .f(() => connection.InitializeSchema("features", "CanInitializeDatabase"));

            "And I query the database"
                .f(() =>
                {
                    using (var command = new SqlCommand("features.CanInitializeDatabase", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        connection.Open();
                        result = int.Parse(command.ExecuteScalar().ToString());
                    }
                });

            "Then the result is expected"
                .f(() => result.Should().Be(42));
        }

        [Scenario]
        public void CanAlterDatabase(SqlConnection connection, Action action)
        {
            "Given a SQL Server connection"
                .f(c => connection = new SqlConnection(this.ConnectionString).Using(c));

            "When I initialize that connection for this test"
                .f(() => action = () => connection.InitializeSchema("features", "CanAlterDatabase"));

            "Then that action should succeed"
                .f(() => action.ShouldNotThrow());
        }
    }
}
