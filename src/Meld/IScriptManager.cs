// <copyright file="IScriptManager.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    using System.Collections.Generic;

    /// <summary>
    /// Exposes the public members of a script manager.
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        /// Gets the SQL scripts.
        /// </summary>
        /// <param name="databaseName">Name of the database.</param>
        /// <returns>The SQL scripts.</returns>
        IEnumerable<SqlScript> GetSqlScripts(string databaseName);

        /// <summary>
        /// Throws a missing script exception.
        /// </summary>
        /// <param name="message">The exception message.</param>
        void ThrowMissingScriptException(string message);
    }
}
