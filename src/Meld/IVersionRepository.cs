// <copyright file="IVersionRepository.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld
{
    internal interface IVersionRepository
    {
        int GetVersion(string databaseName, string schemaName);

        void SetVersion(string databaseName, string schemaName, int version, string description);
    }
}
