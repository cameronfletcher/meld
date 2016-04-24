// <copyright file="DatabaseCollection.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Sdk
{
    using Xunit;

    // LINK (Cameron): http://xunit.github.io/docs/shared-context.html
    [CollectionDefinition("SQL Server Collection")]
    public class DatabaseCollection : ICollectionFixture<SqlServerFixture>
    {
        // This class has no code, and is never created. Its purpose is simply
        // to be the place to apply [CollectionDefinition] and all the
        // ICollectionFixture<> interfaces.
    }
}
