using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GolanAvraham.ConfigurationTransform.Services.Extensions;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class VsServicesTest
    {
        [TestMethod]
        public void IsLinkProjectItem_PropertyExist_Returns_True()
        {
            //Arrange
            var projectItem = new Mock<ProjectItem>();
            var properties = new Mock<Properties>();
            var enumerable = properties.As<IEnumerable>();
            var property = new Mock<Property>();
            property.SetupGet(s => s.Name).Returns("IsLink");
            property.SetupGet(s => s.Value).Returns(true);
            var propertiesEnumerator = new[] { property.Object };
            enumerable.Setup(s => s.GetEnumerator()).Returns(propertiesEnumerator.GetEnumerator());
            projectItem.SetupGet(s => s.Properties).Returns(properties.Object);

            //Act
            var actual = projectItem.Object.IsLink();

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsLinkProjectItem_PropertyExist_Returns_False()
        {
            //Arrange
            var projectItem = new Mock<ProjectItem>();
            var properties = new Mock<Properties>();
            var enumerable = properties.As<IEnumerable>();
            var property = new Mock<Property>();
            property.SetupGet(s => s.Name).Returns("IsLink");
            property.SetupGet(s => s.Value).Returns(false);
            var propertiesEnumerator = new[] { property.Object };
            enumerable.Setup(s => s.GetEnumerator()).Returns(propertiesEnumerator.GetEnumerator());
            projectItem.SetupGet(s => s.Properties).Returns(properties.Object);

            //Act
            var actual = projectItem.Object.IsLink();

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsLinkProjectItem_PropertyNotExist_Returns_False()
        {
            //Arrange
            var projectItem = new Mock<ProjectItem>();
            var properties = new Mock<Properties>();
            var enumerable = properties.As<IEnumerable>();
            var property = new Mock<Property>();
            property.SetupGet(s => s.Name).Returns("MockPropertyName");
            property.SetupGet(s => s.Value).Returns(false);
            var propertiesEnumerator = new[] { property.Object };
            enumerable.Setup(s => s.GetEnumerator()).Returns(propertiesEnumerator.GetEnumerator());
            projectItem.SetupGet(s => s.Properties).Returns(properties.Object);

            //Act
            var actual = projectItem.Object.IsLink();

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void GetBuildConfigurationNames_Returns_BuildArray()
        {
            //Arrange
            var project = new Mock<Project>();
            var configurationManager = new Mock<ConfigurationManager>();
            configurationManager.SetupGet(s => s.ConfigurationRowNames).Returns(new object[] {"debug", "release"});
            project.SetupGet(s => s.ConfigurationManager).Returns(configurationManager.Object);
            //var target = new Mock<VsServices>();

            //Act
            var actual = project.Object.GetBuildConfigurationNames();

            //Assert
            Assert.AreEqual(2,actual.Length);
            Assert.AreEqual("debug", actual[0]);
            Assert.AreEqual("release", actual[1]);
        }

        [TestMethod]
        public void TryRegisterCloseAndDeleteFile_Return_True()
        {
            //Arrange
            const string file = @"c:\file.xml";
            var windowFrameMock = new Mock<IVsWindowFrame>();
            var windowFrame2Mock = windowFrameMock.As<IVsWindowFrame2>();

            var deleteHandlerMock = new Mock<IDeleteFileOnWindowFrameClose>();

            //Act
            var result = windowFrameMock.Object.TryRegisterCloseAndDeleteFile(file, deleteHandlerMock.Object);
            uint cookie;
            //Assert
            Assert.IsTrue(result);
            windowFrame2Mock.Verify(v => v.Advise(deleteHandlerMock.Object, out cookie));
        }

        [TestMethod]
        public void TryRegisterCloseAndDeleteFile_NotCastableTo_IVsWindowFrame2_Return_False()
        {
            //Arrange
            const string file = @"c:\file.xml";

            //Act
            var result = VsServicesExtensions.TryRegisterCloseAndDeleteFile(null, file);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void OpenDiff_OpenComparisonWindow_Throw_Exception()
        {
            //Arrange
            const string leftFile = "leftfile";
            const string rightFile = "rightfile";
            const string leftLabel = "leftlabel";
            const string rightLabel = "rightlabel";
            var mock = new Mock<VsServices>() {CallBase = true};
            mock.Setup(s => s.OpenComparisonWindow(leftFile, rightFile, leftLabel, rightLabel)).Throws<Exception>();

            //Act
            mock.Object.OpenDiff(leftFile, rightFile, leftLabel, rightLabel);

            //Assert
            mock.VerifyAll();
        }
    }
}
