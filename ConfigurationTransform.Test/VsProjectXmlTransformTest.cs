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

        private static string GetFileInUpper(string fileName, string startingFolder)
        {
            if (string.IsNullOrWhiteSpace(startingFolder))
            {
                throw new FileNotFoundException(fileName);
            }
            var combine = Path.Combine(startingFolder, fileName);
            if (File.Exists(combine))
            {
                return combine;
            }
            return GetFileInUpper(fileName, Directory.GetParent(startingFolder).FullName);
        }

        static VsProjectXmlTransformTest()
        {
            var name = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            ProjectWithLinkFiles = XElement.Load(GetFileInUpper("TestProjectWithLinkFiles.xml", name));
            ProjectWithoutLinkFiles = XElement.Load(GetFileInUpper("TestProject.xml", name));
        }

        [TestMethod]
        public void TryGetIncludeConfigElement_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
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
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);

            //Act
            var actual = target.Object.HasTransformTask();

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasAfterPublishTarget_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);

            //Act
            var actual = target.Object.HasAfterPublishTarget();

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasAfterCompileTarget_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
            const string condition = @"Exists('App.$(Configuration).config')";

            //Act
            var actual = target.Object.HasAfterCompileTarget(condition);

            //Assert
            Assert.AreEqual(true, actual);
        }

        [TestMethod]
        public void HasDependentUponConfig_Returns_True()
        {
            //Arrange
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
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
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithLinkFiles);
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
            var target = new Mock<VsProjectXmlTransformMock>("");
            target.Protected().Setup<XElement>("LoadProjectFile").Returns(ProjectWithoutLinkFiles);
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
        public VsProjectXmlTransformMock(string fileName)
            : base(fileName)
        {
        }

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
