using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using CcgVault;
using System.Linq;

namespace Test
{
    [TestClass]
    public class PluginInputTests
    {
        private string validInput = @"Z:\Path\To\config.yml|arg1|arg2|arg3";

        [TestMethod]
        public void Path_Is_Populated()
        {
            var input = new PluginInput(validInput);
            Assert.AreEqual(@"Z:\Path\To\config.yml", input.Path);
        }

        [TestMethod]
        public void Argument_Count_Is_Correct()
        {
            var input = new PluginInput(validInput);
            Assert.AreEqual(3, input.Arguments.Count);
        }

        [TestMethod]
        public void Arguments_Are_Correct()
        {
            var input = new PluginInput(validInput);
            Assert.IsTrue(input.Arguments.SequenceEqual<string>(new[] { "arg1", "arg2", "arg3" }));
        }
    }
}
