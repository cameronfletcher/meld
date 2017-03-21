// <copyright file="SqlDatabaseSchema.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage(
            "Microsoft.Globalization",
            "CA1303:Do not pass literals as localized parameters",
            MessageId = "Meld.IScriptManager.ThrowMissingScriptException(System.String)",
            Justification = "No localization here.")]
        public void Initialize(SqlDatabase sqlDatabase)
        {
            Guard.Against.Null(() => sqlDatabase);

            var versionRepository = new VersionRepository(sqlDatabase.ConnectionString);

            var version = new Version();
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

            var sqlScripts = this.scriptManager.GetSqlScripts(this.databaseName);
            if (!sqlScripts.Any())
            {
                this.scriptManager.ThrowMissingScriptException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot find any SQL scripts for the database named '{0}'.",
                        this.databaseName));
            }

            if (sqlScripts.Any(sqlScript => sqlScript == null))
            {
                throw new ArgumentException("One or more of the scripts returned from the script manager has a null value.");
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
            if (lastSqlScript.Version == version.Number)
            {
                return;
            }

            if (lastSqlScript.Version < version.Number)
            {
                var exception = (SqlException)FormatterServices.GetUninitializedObject(typeof(SqlException));
                typeof(SqlException)
                    .GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(
                        exception,
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "The database version '{0}' is ahead of the database version '{1}' supported by this library.",
                            version.Number,
                            lastSqlScript.Version));

                throw exception;
            }

            sqlDatabase.Execute(sqlScripts.Where(sqlScript => sqlScript.Version > version.Number), this.schemaName);

            version.Apply(sqlScripts);

            versionRepository.SetVersion(this.databaseName, this.schemaName, lastSqlScript.Description, version);
        }
    }
}
