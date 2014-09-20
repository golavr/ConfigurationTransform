using System;
using System.Collections;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design.Behavior;
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
        public void RelativePath_HigherLevel()
        {
            //Arrange
            const string filePath = @"c:\folder1\folder2\folder3\firstproject\file.txt";
            const string referencePath = @"c:\folder1\folder2\folder3\folder4\secondproject\file.txt";
            var expected = @"..\..\firstproject\file.txt";

            //Act
            var actual = ProjectItemExtensions.RelativePath(filePath, referencePath);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativePath_LowerLevel()
        {
            //Arrange
            const string filePath = @"c:\folder1\folder2\folder3\folder4\firstproject\file.txt";
            const string referencePath = @"c:\folder1\folder2\folder3\secondproject\file.txt";
            var expected = @"..\folder4\firstproject\file.txt";

            //Act
            var actual = ProjectItemExtensions.RelativePath(filePath, referencePath);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativePath_SameLevel()
        {
            //Arrange
            const string filePath = @"c:\folder1\folder2\folder3\firstproject\file.txt";
            const string referencePath = @"c:\folder1\folder2\folder3\secondproject\file.txt";
            var expected = @"..\firstproject\file.txt";

            //Act
            var actual = ProjectItemExtensions.RelativePath(filePath, referencePath);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RelativeDirectory()
        {
            //Arrange
            var relativePath = @"..\folder4\firstproject\file.txt";
            var expected = @"..\folder4\firstproject";

            //Act
            var actual = ProjectItemExtensions.RelativeDirectory(relativePath);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Parent_Return_ProjectItem()
        {
            //Arrange
            const string expected = "you_should_find_me";

            var sourceProjectItemMock = new Mock<ProjectItem>();
            var containingProjectItemMock = new Mock<ProjectItem>();
            var firstProjectItemMock = new Mock<ProjectItem>();
            var projectMock = new Mock<Project>();

            // source ProjectItem setup
            sourceProjectItemMock.SetupProperty(item => item.Name, "nameMock");
            sourceProjectItemMock.SetupGet(item => item.ContainingProject).Returns(projectMock.Object);

            // containing ProjectItem setup
            containingProjectItemMock.SetupProperty(item => item.Name, expected);
            var containingProjectItemsMock = (new[] { sourceProjectItemMock.Object }).MockProjectItems();
            containingProjectItemsMock.SetupGet(items => items.Count).Returns(1);
            containingProjectItemMock.SetupGet(item => item.ProjectItems).Returns(containingProjectItemsMock.Object);

            // first ProjectItem
            firstProjectItemMock.SetupProperty(item => item.Name, "firstNameMock");
            var mock = new Mock<ProjectItems>();
            mock.SetupGet(items => items.Count).Returns(0);
            firstProjectItemMock.SetupGet(item => item.ProjectItems).Returns(mock.Object);

            // project setup
            var projectItemsMock = new[] { firstProjectItemMock.Object, containingProjectItemMock.Object }.MockProjectItems();
            projectMock.SetupGet(project => project.ProjectItems).Returns(projectItemsMock.Object);

            //Act
            var actual = sourceProjectItemMock.Object.ParentProjectItemOrDefault();

            //Assert
            Assert.AreEqual(expected, actual.Name);
        }

        [TestMethod]
        public void Parent_Return_Null_ProjectItem()
        {
            //Arrange
            var sourceProjectItemMock = new Mock<ProjectItem>();
            var siblingProjectItemMock = new Mock<ProjectItem>();
            var projectMock = new Mock<Project>();

            var mock = new Mock<ProjectItems>();
            mock.SetupGet(items => items.Count).Returns(0);

            // source ProjectItem setup
            sourceProjectItemMock.SetupProperty(item => item.Name, "mockName");
            sourceProjectItemMock.SetupGet(item => item.ContainingProject).Returns(projectMock.Object);
            sourceProjectItemMock.SetupGet(item => item.ProjectItems).Returns(mock.Object);

            // containing ProjectItem setup
            siblingProjectItemMock.SetupProperty(item => item.Name, "mockName2");
            siblingProjectItemMock.SetupGet(item => item.ProjectItems).Returns(mock.Object);
            //var containingProjectItemsMock = (new[] { siblingProjectItemMock.Object }).MockProjectItems();
            //siblingProjectItemMock.SetupGet(item => item.ProjectItems).Returns(containingProjectItemsMock.Object);

            // project setup
            projectMock.SetupProperty(project => project.Name, "mockProject");
            var projectItemsMock = new[] { sourceProjectItemMock.Object, siblingProjectItemMock.Object }.MockProjectItems();
            projectMock.SetupGet(project => project.ProjectItems).Returns(projectItemsMock.Object);

            //Act
            var actual = sourceProjectItemMock.Object.ParentProjectItemOrDefault();

            //Assert
            Assert.IsNull(actual);
        }
    }
}
