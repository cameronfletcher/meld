// <copyright file="SqlScript.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    /// <summary>
    /// Represents a SQL script.
    /// </summary>
    public class SqlScript
    {
        /// <summary>
        /// Gets or sets the version for the script.
        /// </summary>
        /// <value>The version.</value>
        public int Version { get; set; }

        /// <summary>
        /// Gets or sets the description for the script.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the batches for the script.
        /// </summary>
        /// <value>The batches.</value>
        public string[] Batches { get; set; }
    }
}
