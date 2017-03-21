// <copyright file="ManifestResourceScriptManagerTests.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Unit
{
    using System;
    using System.Linq;
    using FluentAssertions;
    using Xunit;

    public class SqlScriptTests
    {
        [Fact]
        public void CanCreateSqlScript()
        {
            // arrange
            var version = 4;
            var description = "description";
            var sql = "PRINT 'test';";

            // act
            var sqlScript = new SqlScript(version, description, sql);

            // assert
            sqlScript.Version.Should().Be(version);
            sqlScript.Description.Should().Be(description);
            sqlScript.GetSqlBatches("$database", "dbo", "ver").Should().ContainSingle(batch => batch == sql);
            sqlScript.SupportedInTransaction.Should().BeTrue();
        }

        [Fact]
        public void CanCreateSqlScriptWithSchemaCreation()
        {
            // arrange
            var version = 1;
            var description = "description";
            var sql = "PRINT 'test';";

            // act
            var sqlScript = new SqlScript(version, description, sql);

            // assert
            sqlScript.Version.Should().Be(version);
            sqlScript.Description.Should().Be(description);
            sqlScript.GetSqlBatches("$database", "dbo", "ver").Should().ContainInOrder(
                @"IF NOT EXISTS (SELECT * FROM information_schema.schemata WHERE schema_name = 'dbo')
    EXEC sp_executesql N'CREATE SCHEMA [dbo];';",
                sql);
            sqlScript.SupportedInTransaction.Should().BeTrue();
        }

        [Fact]
        public void CanCreateSqlScriptThatIsNotSupportedInTransaction()
        {
            // arrange
            var version = 1;
            var description = "description";
            var sql = @"ALTER DATABASE [$database] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;";

            // act
            var sqlScript = new SqlScript(version, description, sql);

            // assert
            sqlScript.Version.Should().Be(version);
            sqlScript.Description.Should().Be(description);
            sqlScript.GetSqlBatches("$database", "dbo", "ver").Should().ContainSingle(batch => batch == sql);
            sqlScript.SupportedInTransaction.Should().BeFalse();
        }

        [Fact]
        public void CannotCreateSqlScriptThatIsNotSupportedInTransactionWithMultipleSqlBatches()
        {
            // arrange
            var version = 1;
            var description = "description";
            var sql = @"PRINT 'test';
GO
ALTER DATABASE [$database] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
GO";

            // act
            Action action = () => new SqlScript(version, description, sql);

            // assert
            action.ShouldThrow<NotSupportedException>();
        }

        [Fact]
        public void CanCreateSqlScriptBasedOnSqlServerVersion()
        {
            // arrange
            var version = 1;
            var description = "description";
            var sql2008 = "RAISERROR ('Timeout (server side). Failed to acquire read lock for stream.', 16, 50500);";
            var sql2012 = "THROW 50500, 'Timeout (server side). Failed to acquire read lock for stream.', 1;";
            var sql2014 = "-- nothing";
            var sql = string.Concat("#if 10\r\n", sql2008, "\r\n#elseif 11.0\r\n", sql2012, "\r\n#else\r\n", sql2014, "\r\n#endif");

            // act
            var sqlScript = new SqlScript(version, description, sql);

            // assert
            sqlScript.GetSqlBatches("test", "dbo", "10.50.6000").Should().ContainSingle(batch => batch == sql2008);
            sqlScript.GetSqlBatches("test", "dbo", "11.0.6020").Should().ContainSingle(batch => batch == sql2012);
            sqlScript.GetSqlBatches("test", "dbo", "12.0.5540").Should().ContainSingle(batch => batch == sql2014);
        }
    }
}