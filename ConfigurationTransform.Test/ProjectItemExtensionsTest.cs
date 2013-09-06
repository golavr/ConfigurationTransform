using System;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GolanAvraham.ConfigurationTransform.Services.Extensions;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class ProjectItemExtensionsTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var target = new Mock<ProjectItem>() {CallBase = true};
        }
    }
}
