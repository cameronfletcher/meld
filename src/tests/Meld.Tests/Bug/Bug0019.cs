// <copyright file="Bug0019.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Bug
{
    using System.Linq;
    using Xunit;

    public class Bug0019
    {
        [Fact]
        public void CanReferenceDuplicateScript()
        {
            // arrange
            Support.Ensure.Loaded(); // NOTE (Cameron): This ensures that the assembly is loaded which affect the script manager (pre bug fix).
            var scriptManager = new ManifestResourceScriptManager();

            // act
            var scripts = scriptManager.GetSqlScripts("Duplicate");

            // assert
            Assert.True(scripts.Count() == 1);
        }
    }
}
