// <copyright file="ManifestResourceScriptManagerTests.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Unit
{
    using System;
    using System.Resources;
    using FluentAssertions;
    using Xunit;

    public class ManifestResourceScriptManagerTests
    {
        [Fact]
        public void Test()
        {
            var scriptManager = new ManifestResourceScriptManager();
            var message = "message";

            Action action = () => scriptManager.ThrowMissingScriptException(message);

            action.ShouldThrow<MissingManifestResourceException>().WithMessage(message);
        }
    }
}
