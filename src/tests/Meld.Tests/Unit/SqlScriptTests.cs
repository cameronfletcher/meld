// <copyright file="ManifestResourceScriptManagerTests.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Unit
{
    using System;
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
            sqlScript.GetSqlBatches("$database", "dbo").Should().ContainSingle(batch => batch == sql);
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
            sqlScript.GetSqlBatches("$database", "dbo").Should().ContainInOrder(
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
            sqlScript.GetSqlBatches("$database", "dbo").Should().ContainSingle(batch => batch == sql);
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
    }
}
