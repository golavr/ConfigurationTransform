using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GolanAvraham.ConfigurationTransform.Services.Extensions;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class VsServicesTest
    {
        //[TestMethod]
        //public void GetSelectedFileName_Returns_AppConfigFileName()
        //{
        //    //Arrange
        //    var target = new Mock<VsServices>();
        //    var dte = new Mock<DTE>();
        //    var selectedItems = new Mock<SelectedItems>();
        //    var selectedItem = new Mock<SelectedItem>();
        //    const string expected = "app.config";
        //    selectedItem.SetupGet(s => s.Name).Returns(expected);
        //    selectedItems.Setup(s => s.Item(It.IsAny<object>())).Returns(selectedItem.Object);
        //    selectedItems.SetupGet(s => s.Count).Returns(1);
        //    dte.Setup(s => s.SelectedItems).Returns(selectedItems.Object);
        //    target.SetupGet(s => s.Dte).Returns(dte.Object);

        //    //Act
        //    var actual = target.Object.GetSelectedFileName();

        //    //Assert
        //    Assert.AreEqual(expected, actual);
        //}

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

        //[TestMethod]
        //public void GetSelectedFileName_MultiFilesSelected_Returns_Null()
        //{
        //    //Arrange
        //    var target = new Mock<VsServices>();
        //    var dte = new Mock<DTE>();
        //    var selectedItems = new Mock<SelectedItems>();
        //    selectedItems.SetupGet(s => s.Count).Returns(2);
        //    dte.Setup(s => s.SelectedItems).Returns(selectedItems.Object);
        //    target.SetupGet(s => s.Dte).Returns(dte.Object);

        //    //Act
        //    var actual = target.Object.GetSelectedFileName();

        //    //Assert
        //    Assert.IsNull(actual);
        //}

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
    }
}
