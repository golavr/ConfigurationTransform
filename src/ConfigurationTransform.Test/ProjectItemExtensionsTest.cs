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
            var parentProjectItemMock = new Mock<ProjectItem>();
            
            var mock = new Mock<ProjectItems>();
            mock.SetupGet(items => items.Count).Returns(0);
            mock.SetupGet(parent => parent.Parent).Returns(parentProjectItemMock.Object);

            parentProjectItemMock.SetupProperty(p => p.Name, expected);

            sourceProjectItemMock.SetupGet(c => c.Collection).Returns(mock.Object);

            //Act
            var actual = sourceProjectItemMock.Object.ParentProjectItemOrDefault();

            //Assert
            Assert.AreEqual(expected, actual.Name);
        }

        [TestMethod]
        public void Parent_Return_Null_ProjectItem_For_Project()
        {
            //Arrange
            var sourceProjectItemMock = new Mock<ProjectItem>();
            var parentProjectMock = new Mock<Project>();

            var mock = new Mock<ProjectItems>();
            mock.SetupGet(items => items.Count).Returns(0);
            mock.SetupGet(parent => parent.Parent).Returns(parentProjectMock.Object);

            //Act
            var actual = sourceProjectItemMock.Object.ParentProjectItemOrDefault();

            //Assert
            Assert.IsNull(actual);
        }
    }
}
