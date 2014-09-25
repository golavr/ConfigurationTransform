using System;
using System.IO;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class DeleteFileOnWindowFrameCloseTest
    {
        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void Constractor_ThrowsArgumentException_WhenFilePathIsNull()
        {
            //Act
            var deleteFileOnWindowFrameClose = new DeleteFileOnWindowFrameClose(null);
        }

        [ExpectedException(typeof(ArgumentException))]
        [TestMethod]
        public void Constractor_ThrowsArgumentException_WhenFilePathIsEmpty()
        {
            //Act
            var deleteFileOnWindowFrameClose = new DeleteFileOnWindowFrameClose(string.Empty);
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
            var deleteFileOnWindowFrameClose = new DeleteFileOnWindowFrameClose("file");
        }

        [TestMethod]
        public void OnShow_AsClose_DeleteFile()
        {
            //Arrange
            const string path = @"c:\file.xml";
            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(true);
            var target = new DeleteFileOnWindowFrameClose(path, fileWrapperMock.Object);

            //Act
            target.OnShow((int) __FRAMESHOW.FRAMESHOW_WinClosed);

            //Assert
            fileWrapperMock.VerifyAll();
            fileWrapperMock.Verify(v=>v.Delete(path));
        }

        [TestMethod]
        public void OnShow_AsClose_FileNotExist_ReturnNoError()
        {
            //Arrange
            const string path = @"c:\file.xml";
            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(true);
            var target = new DeleteFileOnWindowFrameClose(path, fileWrapperMock.Object);
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(false);

            //Act
            target.OnShow((int)__FRAMESHOW.FRAMESHOW_WinClosed);

            //Assert
            fileWrapperMock.VerifyAll();
            fileWrapperMock.Verify(v => v.Delete(path), Times.Never());
        }
    }
}