// <copyright file="SqlDatabaseSchema.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Serialization;

    internal class SqlDatabaseSchema
    {
        private readonly IScriptManager scriptManager = new ManifestResourceScriptManager();

        private readonly string databaseName;
        private readonly string schemaName;

        public SqlDatabaseSchema(string databaseName, string schemaName)
        {
            Guard.Against.Null(() => databaseName);
            Guard.Against.Null(() => schemaName);

            this.databaseName = databaseName;
            this.schemaName = schemaName;
        }

        public void Initialize(SqlDatabase sqlDatabase)
        {
            Guard.Against.Null(() => sqlDatabase);

            var versionRepository = new VersionRepository(sqlDatabase.ConnectionString);

            var version = 0;
            try
            {
                version = versionRepository.GetVersion(this.databaseName, this.schemaName);
            }
            catch (SqlException ex)
            {
                if (!ex.Errors.Cast<SqlError>().Any(error => error.Number == 2812))
                {
                    throw;
                }
            }

            var sqlScripts = this.scriptManager.GetSqlScripts(this.databaseName, this.schemaName);
            if (!sqlScripts.Any())
            {
                this.scriptManager.ThrowMissingScriptException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot find any SQL scripts for the database named '{0}'.",
                        this.databaseName));
            }

            if (sqlScripts.Min(script => script.Version != 1))
            {
                this.scriptManager.ThrowMissingScriptException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot find version one of the SQL script for the database named '{0}'.",
                        this.databaseName));
            }

            // LINK (Cameron): http://stackoverflow.com/questions/13359327/check-if-listint32-values-are-consecutive
            if (sqlScripts.Select(script => script.Version).Select((v1, v2) => v1 - v2).Distinct().Skip(1).Any())
            {
                this.scriptManager.ThrowMissingScriptException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "The SQL scripts for the database named '{0}' are non-contiguous.",
                        this.databaseName));
            }

            var lastSqlScript = sqlScripts.Last();
            if (lastSqlScript.Version == version)
            {
                return;
            }

            if (lastSqlScript.Version < version)
            {
                var exception = (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));
                typeof(SqlException)
                    .GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(
                        exception,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The database version '{0}' is ahead of the database version '{1}' supported by this library.",
                            version,
                            lastSqlScript.Version));

                throw exception;
            }

            sqlDatabase.Execute(sqlScripts.Where(sqlScript => sqlScript.Version > version));

            versionRepository.SetVersion(this.databaseName, this.schemaName, lastSqlScript.Version, lastSqlScript.Description);
        }
    }
}
