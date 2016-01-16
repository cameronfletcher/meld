// <copyright file="ManifestResourceScriptManager.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents the manifest resource script manager.
    /// </summary>
    /// <seealso cref="Meld.IScriptManager" />
    public class ManifestResourceScriptManager : IScriptManager
    {
        /// <summary>
        /// Gets the SQL scripts.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <returns>The SQL scripts.</returns>
        public IEnumerable<SqlScript> GetSqlScripts(string databaseName, string schemaName)
        {
            Guard.Against.NullOrEmpty(() => databaseName);
            Guard.Against.NullOrEmpty(() => schemaName);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    assembly =>
                    SafeGetManifestResourceNames(assembly)
                        .Where(name =>
                            name.StartsWith(string.Concat(assembly.GetName().Name, ".Scripts.", databaseName), StringComparison.OrdinalIgnoreCase))
                        .Select(
                            resourceName =>
                            new
                            {
                                Assembly = assembly,
                                ResourceName = resourceName,
                            }))
                .Select(
                    sqlScript =>
                    new SqlScript
                    {
                        Version = GetSqlScriptVersion(sqlScript.Assembly, sqlScript.ResourceName, databaseName),
                        Description = GetSqlScriptDescription(sqlScript.Assembly),
                        Batches = GetSqlScriptBatches(sqlScript.Assembly, sqlScript.ResourceName, schemaName),
                    })
                .OrderBy(sqlScript => sqlScript.Version)
                .ToArray();
        }

        /// <summary>
        /// Throws a missing script exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <exception cref="System.Resources.MissingManifestResourceException">Thrown when invoked.</exception>
        public void ThrowMissingScriptException(string message)
        {
            throw new MissingManifestResourceException(message);
        }

        private static int GetSqlScriptVersion(Assembly assembly, string resourceName, string databaseName)
        {
            return int.Parse(
                resourceName
                    .Replace(string.Concat(assembly.GetName().Name, ".Scripts.", databaseName), string.Empty)
                    .Replace(".sql", string.Empty),
                CultureInfo.InvariantCulture);
        }

        private static string GetSqlScriptDescription(Assembly assembly)
        {
            var assemblyName = assembly.GetName();
            var versionAttribute = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false)
                .Cast<AssemblyInformationalVersionAttribute>()
                .FirstOrDefault();

            return string.Concat(
                assemblyName.Name,
                " ",
                versionAttribute == null ? assemblyName.Version.ToString() : versionAttribute.InformationalVersion);
        }

        // LINK (Cameron): http://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        [SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "It's OK here.")]
        private static string[] GetSqlScriptBatches(Assembly assembly, string resourceName, string schemaName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return Regex
                    .Split(
                        reader.ReadToEnd(),
                        @"^\s*GO\s* ($ | \-\- .*$)",
                        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase)
                    .Where(sqlScript => !string.IsNullOrWhiteSpace(sqlScript))
                    .Select(sqlScript => sqlScript.Trim(' ', '\r', '\n'))
                    .Select(sqlScript => ReplaceSchema(sqlScript, schemaName))
                    .ToArray();
            }
        }

        // TODO (Cameron): This is a mess, so reg-ex? lol
        private static string ReplaceSchema(string sqlScriptBatch, string schemaName)
        {
            return sqlScriptBatch
                .Replace("[dbo]", string.Concat("[", schemaName, "]"))
                .Replace(" dbo.", string.Concat(" ", schemaName, "."))
                .Replace("'dbo.", string.Concat("'", schemaName, "."))
                .Replace("'dbo'", string.Concat("'", schemaName, "'"));
        }

        // HACK (Cameron): This is a brute force approach to fix inability to load manifest resources from dynamic assemblies. It should be rewritten.
        private static string[] SafeGetManifestResourceNames(Assembly assembly)
        {
            try
            {
                return assembly.GetManifestResourceNames();
            }
            catch (NotSupportedException)
            {
                return new string[0];
            }
        }
    }
}
