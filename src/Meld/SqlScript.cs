// <copyright file="SqlScript.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents a SQL script.
    /// </summary>
    public class SqlScript
    {
        // NOTE (Cameron): You can use all Transact-SQL statements in an explicit transaction, except for the following statements:
        // LINK (Cameron): https://technet.microsoft.com/en-us/library/ms191544(v=sql.105).aspx
        private static readonly string[] Statements = new[]
        {
            "ALTER DATABASE",
            "CREATE FULLTEXT INDEX",
            "ALTER FULLTEXT CATALOG",
            "DROP DATABASE",
            "ALTER FULLTEXT INDEX",
            "DROP FULLTEXT CATALOG",
            "BACKUP",
            "DROP FULLTEXT INDEX",
            "CREATE DATABASE",
            "RECONFIGURE",
            "CREATE FULLTEXT CATALOG",
            "RESTORE",
        };

        private readonly List<string> sqlBatches;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScript"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="description">The description.</param>
        /// <param name="sqlScript">The SQL script.</param>
        public SqlScript(int version, string description, string sqlScript)
            : this(version, description, GetSqlScriptBatches(sqlScript))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScript"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="description">The description.</param>
        /// <param name="sqlBatches">The SQL batches.</param>
        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "It's fine here.")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", Justification = "URL")]
        internal SqlScript(int version, string description, IEnumerable<string> sqlBatches)
        {
            Guard.Against.NullOrEmptyOrNullElements(() => sqlBatches);

            this.Version = version;
            this.Description = description;
            this.sqlBatches = new List<string>(sqlBatches);
            this.SupportedInTransaction =
                !this.sqlBatches.Any(sqlBatch => Statements.Any(statement => sqlBatch.IndexOf(statement, StringComparison.OrdinalIgnoreCase) >= 0));

            if (this.sqlBatches.Count > 1 && !this.SupportedInTransaction)
            {
                throw new NotSupportedException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        @"Cannot create a SQL script comprised of multiple batches some of which are not supported in transactions.
To fix, please isolate any batches containing the following statements into individual scripts:
{0}
For more information: https://technet.microsoft.com/en-us/library/ms191544(v=sql.105).aspx",
                        string.Join(",\r\n", Statements)));
            }

            // HACK (Cameron): This does not attempt to create a schema if the scripts contain one of the statements that is not supported in transactions.
            if (this.SupportedInTransaction)
            {
                this.sqlBatches.Insert(
                    0,
                    @"IF NOT EXISTS (SELECT * FROM information_schema.schemata WHERE schema_name = 'dbo')
    EXEC sp_executesql N'CREATE SCHEMA [dbo];';");
            }
        }

        private enum Directives
        {
            None = 0,
            IfBlock = 1,
            ElseIfBlock = 2,
            ElseBlock = 3,
        }

        /// <summary>
        /// Gets the version for the script.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; private set; }

        /// <summary>
        /// Gets the description for the script.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this script is supported in a transaction.
        /// </summary>
        /// <value>Returns <c>true</c> if the script is supported in a transaction; otherwise, <c>false</c>.</value>
        public bool SupportedInTransaction { get; private set; }

        /// <summary>
        /// Gets the SQL batches.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <param name="schemaName">Name of the schema.</param>
        /// <param name="serverVersion">The server version.</param>
        /// <returns>The SQL batches.</returns>
        public IEnumerable<string> GetSqlBatches(string databaseName, string schemaName, string serverVersion)
        {
            return this.sqlBatches
                .Select(sqlBatch => ReplaceDatabase(sqlBatch, databaseName))
                .Select(sqlBatch => ReplaceSchema(sqlBatch, schemaName))
                .Select(sqlBatch => ProcessPragamaDirectives(sqlBatch, serverVersion))
                .ToArray();
        }

        // LINK (Cameron): http://stackoverflow.com/questions/18596876/go-statements-blowing-up-sql-execution-in-net
        private static string[] GetSqlScriptBatches(string sql)
        {
            return Regex
                .Split(
                    sql,
                    @"^\s*GO\s* ($ | \-\- .*$)",
                    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase)
                .Where(sqlScript => !string.IsNullOrWhiteSpace(sqlScript))
                .Select(sqlScript => sqlScript.TrimStart('\n').TrimEnd(' ', '\r', '\n'))
                .ToArray();
        }

        private static string ReplaceDatabase(string sqlScriptBatch, string databaseName)
        {
            return sqlScriptBatch
                .Replace("[$database]", string.Concat("[", databaseName, "]"))
                .Replace(" $database.", string.Concat(" ", databaseName, "."))
                .Replace("'$database.", string.Concat("'", databaseName, "."))
                .Replace("'$database'", string.Concat("'", databaseName, "'"));
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

        // NOTE (Cameron): This should probably be split out elsewhere.
        private static string ProcessPragamaDirectives(string sqlScriptBatch, string serverVersion)
        {
            var stringBuilder = new StringBuilder();
            var currentDirective = default(Directives);
            var condition = true;
            var conditionSatisfied = true;

            foreach (var line in sqlScriptBatch.Split(new[] { "\r\n" }, StringSplitOptions.None))
            {
                var trimmedLine = line.Trim();

                if (trimmedLine.StartsWith("#", StringComparison.OrdinalIgnoreCase))
                {
                    var directive = default(Directives);

                    // NOTE (Cameron): Directive parsing...
                    if (trimmedLine.StartsWith("#if ", StringComparison.OrdinalIgnoreCase) && trimmedLine.Length > 4)
                    {
                        directive = Directives.IfBlock;
                        var versionSubstring = trimmedLine.Substring(4, Math.Min(serverVersion.Length, trimmedLine.Length - 4));
                        condition = conditionSatisfied = serverVersion.StartsWith(versionSubstring, StringComparison.OrdinalIgnoreCase);
                    }
                    else if (trimmedLine.StartsWith("#elseif ", StringComparison.OrdinalIgnoreCase) && trimmedLine.Length > 8)
                    {
                        directive = Directives.ElseIfBlock;
                        var versionSubstring = trimmedLine.Substring(8, Math.Min(serverVersion.Length, trimmedLine.Length - 8));
                        condition = serverVersion.StartsWith(versionSubstring, StringComparison.OrdinalIgnoreCase);
                        conditionSatisfied = conditionSatisfied || condition;
                    }
                    else if (trimmedLine.StartsWith("#else", StringComparison.OrdinalIgnoreCase) && trimmedLine.Length == 5)
                    {
                        directive = Directives.ElseBlock;
                        condition = !conditionSatisfied;
                    }
                    else if (trimmedLine.StartsWith("#endif", StringComparison.OrdinalIgnoreCase) && trimmedLine.Length == 6)
                    {
                        directive = Directives.None;
                        condition = conditionSatisfied = true;
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format(CultureInfo.InvariantCulture, "Unable to parse directive: '{0}'.", trimmedLine));
                    }

                    // NOTE (Cameron): Program flow...
                    switch (currentDirective)
                    {
                        case Directives.None:
                            if (directive == Directives.IfBlock)
                            {
                                break;
                            }

                            goto default;

                        case Directives.IfBlock:
                        case Directives.ElseIfBlock:
                            if (directive == Directives.ElseIfBlock || directive == Directives.ElseBlock || directive == Directives.None)
                            {
                                break;
                            }

                            goto default;

                        case Directives.ElseBlock:
                            if (directive == Directives.None)
                            {
                                break;
                            }

                            goto default;

                        default:
                            throw new InvalidOperationException(
                                string.Format(CultureInfo.InvariantCulture, "Unable to process directive (unexpected order): '{0}'.", trimmedLine));
                    }

                    currentDirective = directive;
                }
                else
                {
                    if (condition)
                    {
                        stringBuilder.AppendLine(line);
                    }
                }
            }

            return stringBuilder.ToString();
        }

        internal class Comparer : IEqualityComparer<SqlScript>
        {
            public bool Equals(SqlScript x, SqlScript y)
            {
                if (object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (object.ReferenceEquals(x, null) || object.ReferenceEquals(y, null))
                {
                    return false;
                }

                return x.Version == y.Version && x.sqlBatches.SequenceEqual(y.sqlBatches);
            }

            public int GetHashCode(SqlScript obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                unchecked
                {
                    var hashCode = 17;
                    hashCode = (hashCode * 23) + obj.Version.GetHashCode();

                    foreach (var sqlBatch in obj.sqlBatches)
                    {
                        hashCode = (hashCode * 23) + sqlBatch.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }
    }
}
