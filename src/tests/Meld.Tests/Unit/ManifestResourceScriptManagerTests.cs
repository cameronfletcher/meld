// <copyright file="ManifestResourceScriptManagerTests.cs" company="Meld contributors">
//  Copyright (c) Meld contributors. All rights reserved.
// </copyright>

namespace Meld.Tests.Unit
{
    using System;
    using System.CodeDom.Compiler;
    using System.Resources;
    using FluentAssertions;
    using Xunit;

    public class ManifestResourceScriptManagerTests
    {
        [Fact]
        public void CanThrowMissingScriptException()
        {
            // arrange
            var scriptManager = new ManifestResourceScriptManager();
            var message = "message";

            // act
            Action action = () => scriptManager.ThrowMissingScriptException(message);

            // assert
            action.ShouldThrow<MissingManifestResourceException>().WithMessage(message);
        }

        [Fact(Skip = "Not sure how to load a dynamic assembly into the current application domain...")]
        public void DoesNotThrowWhenDynamicAssemblyIsLoaded()
        {
            var parameters = new CompilerParameters { GenerateExecutable = false, OutputAssembly = "autogen.dll" };
            var dynamicAssembly = CodeDomProvider.CreateProvider("CSharp").CompileAssemblyFromSource(parameters, "public class B { public static int k=7; }");

            var b = AppDomain.CurrentDomain.CreateInstanceFromAndUnwrap(dynamicAssembly.PathToAssembly, "B");

            var scriptManager = new ManifestResourceScriptManager();
            var x = scriptManager.GetSqlScripts("test", "dbo");
        }

        [Fact]
        public void CanGetScripts()
        {
            // arrange
            var scriptManager = new ManifestResourceScriptManager();

            // act
            var scripts = scriptManager.GetSqlScripts("a", "dbo");

            // assert
            scripts.Should().HaveCount(2);
        }
    }
}
