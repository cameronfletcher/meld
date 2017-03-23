// <copyright file="Version.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a version.
    /// </summary>
    public class Version
    {
        private readonly Dictionary<int, string> originalSqlBatches = new Dictionary<int, string>();
        private readonly Dictionary<int, SqlScript> sqlBatches = new Dictionary<int, SqlScript>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Version"/> class.
        /// </summary>
        public Version()
            : this(new Dictionary<int, string>())
        {
        }

        internal Version(Dictionary<int, string> sqlBatches)
        {
            var number = 0;

            foreach (var sqlBatch in sqlBatches)
            {
                if (sqlBatch.Key > number)
                {
                    number = sqlBatch.Key;
                }

                this.originalSqlBatches.Add(sqlBatch.Key, sqlBatch.Value);
            }

            this.Number = number;
        }

        /// <summary>
        /// Gets the version number.
        /// </summary>
        /// <value>The version number.</value>
        public int Number { get; private set; }

        /// <summary>
        /// Applies the specified SQL scripts.
        /// </summary>
        /// <param name="sqlScripts">The SQL scripts.</param>
        public void Apply(IEnumerable<SqlScript> sqlScripts)
        {
            Guard.Against.Null(() => sqlScripts);

            var number = 0;

            foreach (var sqlScript in sqlScripts)
            {
                var sqlBatch = default(string);
                if (!this.originalSqlBatches.TryGetValue(sqlScript.Version, out sqlBatch) ||
                    sqlBatch == null)
                {
                    if (sqlScript.Version > number)
                    {
                        number = sqlScript.Version;
                    }

                    this.sqlBatches.Add(sqlScript.Version, sqlScript);
                }
            }

            if (number > this.Number)
            {
                this.Number = number;
            }
        }

        /// <summary>
        /// Gets the SQL scripts.
        /// </summary>
        /// <returns>The SQL scripts.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Inappropriate.")]
        public IEnumerable<SqlScript> GetSqlScripts()
        {
            return this.sqlBatches.OrderBy(e => e.Key).Select(e => e.Value);
        }
    }
}
