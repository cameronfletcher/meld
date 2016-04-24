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
        private static readonly IEqualityComparer<SqlScript> Comparer = new SqlScript.Comparer();

        /// <summary>
        /// Gets the SQL scripts.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The SQL scripts.</returns>
        public IEnumerable<SqlScript> GetSqlScripts(string databaseName)
        {
            Guard.Against.NullOrEmpty(() => databaseName);

            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(
                    assembly =>
                    SafeGetManifestResourceNames(assembly)
                        .Where(name =>
                            name.StartsWith(string.Concat(assembly.GetName().Name, ".Scripts.", databaseName), StringComparison.OrdinalIgnoreCase) ||
                            name.StartsWith(string.Concat("Meld.Scripts.", databaseName), StringComparison.OrdinalIgnoreCase))
                        .Select(
                            resourceName =>
                            new
                            {
                                Assembly = assembly,
                                ResourceName = resourceName,
                            }))
                .Select(
                    sqlScript =>
                    new SqlScript(
                        GetSqlScriptVersion(sqlScript.Assembly, sqlScript.ResourceName, databaseName),
                        GetSqlScriptDescription(sqlScript.Assembly),
                        GetSqlScript(sqlScript.Assembly, sqlScript.ResourceName)))
                .Distinct(Comparer)
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
            var versionString = resourceName
                .Replace(string.Concat(assembly.GetName().Name, ".Scripts.", databaseName), string.Empty)
                .Replace(string.Concat("Meld.Scripts.", databaseName), string.Empty)
                .Replace(".sql", string.Empty);

            var version = 0;
            if (!int.TryParse(versionString, NumberStyles.Any, CultureInfo.InvariantCulture, out version))
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Cannot infer the version number for the script named '{0}'.",
                        resourceName));
            }

            return version;
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
        private static string GetSqlScript(Assembly assembly, string resourceName)
        {
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        // HACK (Cameron): This is a brute force approach to fix inability to load manifest resources from dynamic assemblies. It should be rewritten.
        private static string[] SafeGetManifestResourceNames(Assembly assembly)
        {
            if (assembly.IsDynamic)
            {
                return new string[0];
            }

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
