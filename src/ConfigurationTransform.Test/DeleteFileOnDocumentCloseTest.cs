using System;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class DeleteFileOnDocumentCloseTest
    {
        [TestMethod]
        public void DocumentClosing_DeleteFile_UnRegisterEvent()
        {
            //Arrange
            var documentEvents = new Mock<DocumentEvents>();
            const string filePath = @"c:\file.xml";
            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(s => s.Exists(filePath)).Returns(true);
            var documentMock = new Mock<Document>();
            documentMock.SetupGet(s => s.FullName).Returns(filePath);
            var targetMock = new Mock<DeleteFileOnDocumentClose>(filePath, documentEvents.Object, fileWrapperMock.Object);
            var deleteFileOnDocumentClose = targetMock.Object;
            //Act
            documentEvents.Raise(events =>
            {
                events.DocumentClosing += document => { };
            }, documentMock.Object);

            documentEvents.Raise(events =>
            {
                events.DocumentClosing += document => { };
            }, documentMock.Object);

            //Assert
            targetMock.Verify(v=>v.DeleteFile(), Times.Exactly(1));
        }
    }
}