using System;
using System.IO;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class DeleteFileOnCloseTest
    {
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void Constractor_ThrowsArgumentException_WhenFilePathIsNull()
        {
            //Act
            var deleteFileOnWindowFrameClose = new DeleteFileOnClose(null);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void Constractor_ThrowsArgumentException_WhenFilePathIsEmpty()
        {
            //Act
            var deleteFileOnWindowFrameClose = new DeleteFileOnClose(string.Empty);
        }

        [ExpectedException(typeof(FileNotFoundException))]
        [TestMethod]
        public void Constractor_ThrowsException_WhenFilePathIsEmpty()
        {
            //Arrange
            const string path = @"c:\file.xml";
            var mock = new Mock<IFileWrapper>();
            mock.Setup(s => s.Exists(path)).Returns(false);

            //Act
            var deleteFileOnWindowFrameClose = new DeleteFileOnClose("file");
        }

        [TestMethod]
        public void Delete_FileNotExist_ReturnNoError()
        {
            //Arrange
            const string path = @"c:\file.xml";
            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(true);
            var target = new DeleteFileOnClose(path, fileWrapperMock.Object);
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(false);

            //Act
            target.DeleteFile();

            //Assert
            fileWrapperMock.VerifyAll();
            fileWrapperMock.Verify(v => v.Delete(path), Times.Never());
        }
    }
}