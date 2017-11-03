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
        [TestMethod]
        public void OnShow_AsClose_Call_DeleteFile()
        {
            //Arrange
            const string path = @"c:\file.xml";
            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(s => s.Exists(path)).Returns(true);
            var targetMock = new Mock<DeleteFileOnWindowFrameClose>(path, fileWrapperMock.Object);
            var target = targetMock.Object;

            //Act
            target.OnShow((int)__FRAMESHOW.FRAMESHOW_WinClosed);

            //Assert
            targetMock.Verify(v => v.DeleteFile());
        }
    }
}