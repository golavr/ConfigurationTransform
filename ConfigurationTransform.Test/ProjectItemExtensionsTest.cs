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
    }
}
