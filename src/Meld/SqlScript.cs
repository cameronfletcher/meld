// <copyright file="SqlScript.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Represents a SQL script.
    /// </summary>
    public class SqlScript
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SqlScript"/> class.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="description">The description.</param>
        /// <param name="sqlBatches">The SQL batches.</param>
        public SqlScript(int version, string description, IEnumerable<string> sqlBatches)
        {
            Guard.Against.NullOrEmptyOrNullElements(() => sqlBatches);

            this.Version = version;
            this.Description = description;
            this.SqlBatches = sqlBatches.ToArray();
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
        /// Gets the SQL batches for the script.
        /// </summary>
        /// <value>The SQL batches.</value>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "By design.")]
        public string[] SqlBatches { get; private set; }
    }
}
