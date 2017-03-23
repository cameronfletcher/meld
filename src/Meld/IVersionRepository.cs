// <copyright file="IVersionRepository.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    internal interface IVersionRepository
    {
        Version GetVersion(string databaseName, string schemaName);

        void SetVersion(string databaseName, string schemaName, string description, Version version);
    }
}
