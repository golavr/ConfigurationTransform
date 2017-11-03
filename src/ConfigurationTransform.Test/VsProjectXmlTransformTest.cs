using System.IO;
using System.Linq;
using System.Reflection;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml.Linq;
using Moq;
using Moq.Protected;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class VsProjectXmlTransformTest
    {
        private static readonly XElement ProjectWithLinkFiles;
        private static readonly XElement ProjectWithoutLinkFiles;

        static VsProjectXmlTransformTest()
        {
            var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ProjectWithLinkFiles = XElement.Load(Path.Combine(root, "TestProjectWithLinkFiles.xml"));
            ProjectWithoutLinkFiles = XElement.Load(Path.Combine(root, "TestProject.xml"));
        }

        [TestMethod]
        public void TryGetIncludeConfigElement_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");
            const string appConfigName = "App.config";
            XElement element;

            //Act
            var actual = target.Object.TryGetIncludeConfigElement(appConfigName, out element);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasTransformTask_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");

            //Act
            var actual = target.Object.HasTransformTask();

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasAfterPublishTarget_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");

            //Act
            var actual = target.Object.HasAfterPublishTarget();

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasAfterCompileTarget_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>(){ CallBase = true};
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");
            const string condition = @"Exists('App.$(Configuration).config')";

            //Act
            var actual = target.Object.HasAfterCompileTarget(condition);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasAfterBuildTarget_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>(){ CallBase = true};
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");
            const string condition = @"Exists('log.$(Configuration).config')";

            //Act
            var actual = target.Object.HasAfterBuildTarget(condition);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasDependentUponConfig_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");
            const string buildConfig = "App.Debug.config";

            //Act
            var actual = target.Object.HasDependentUponConfig(buildConfig);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasDependentUponConfigLink_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithLinkFiles);
            target.Object.Open("");
            const string buildConfig = "App.Debug.config";

            //Act
            var actual = target.Object.HasDependentUponConfigLink(buildConfig);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasDependentUponConfigLink_Returns_False()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>() { CallBase = true };
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            target.Object.Open("");
            const string buildConfig = "App.Debug.config";

            //Act
            var actual = target.Object.HasDependentUponConfigLink(buildConfig);

            //Assert
            Assert.AreEqual(false, actual);
        }
    }

    /// <summary>
    /// Wrapper for testing protected methods.
    /// </summary>
    public class VsProjectXmlTransformMock : VsProjectXmlTransform
    {
        //public VsProjectXmlTransformMock(string fileName)
        //    : base(fileName)
        //{
        //}

        public bool HasTransformTask()
        {
            return base.HasTransformTask();
        }

        public bool HasDependentUponConfigLink(string buildConfig)
        {
            return base.HasDependentUponConfigLink(buildConfig);
        }

        public bool HasDependentUponConfig(string buildConfig)
        {
            return base.HasDependentUponConfig(buildConfig);
        }

        public bool HasAfterCompileTarget(string condition)
        {
            return base.HasAfterCompileTarget(condition);
        }

        public bool HasAfterBuildTarget(string condition)
        {
            return base.HasAfterBuildTarget(condition);
        }

        public bool HasAfterPublishTarget()
        {
            return base.HasAfterPublishTarget();
        }

        public bool TryGetIncludeConfigElement(string appConfigName, out XElement element)
        {
            return base.TryGetIncludeConfigElement(appConfigName, out element);
        }
    }
}
