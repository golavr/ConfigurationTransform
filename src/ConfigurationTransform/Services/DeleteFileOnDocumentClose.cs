using System;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Wrappers;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class DeleteFileOnDocumentClose : DeleteFileOnClose
    {
        private readonly DocumentEvents _documentEvents;

        public DeleteFileOnDocumentClose(string filePath, DocumentEvents documentEvents)
            : this(filePath, documentEvents, new FileWrapper())
        {
        }

        public DeleteFileOnDocumentClose(string filePath, DocumentEvents documentEvents, IFileWrapper fileWrapper)
            : base(filePath, fileWrapper)
        {
            _documentEvents = documentEvents;
            documentEvents.DocumentClosing += DocumentEvents_DocumentClosing;
        }

        private void DocumentEvents_DocumentClosing(Document document)
        {
            if (document.FullName != FilePath) return;
            _documentEvents.DocumentClosing -= DocumentEvents_DocumentClosing;
            DeleteFile();
        }

        public static void Register(string filePath, DocumentEvents documentEvents)
        {
            new DeleteFileOnDocumentClose(filePath, documentEvents);
        }
    }
}