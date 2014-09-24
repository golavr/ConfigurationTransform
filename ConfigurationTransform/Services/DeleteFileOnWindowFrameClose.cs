using System;
using System.IO;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class DeleteFileOnWindowFrameClose : IDeleteFileOnWindowFrameClose
    {
        private readonly IFileWrapper _fileWrapper;
        public string FilePath { get; private set; }

        public DeleteFileOnWindowFrameClose(string filePath) : this(filePath, new FileWrapper())
        {
        }

        public DeleteFileOnWindowFrameClose(string filePath, IFileWrapper fileWrapper)
        {
            _fileWrapper = fileWrapper;
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(filePath);
            if (!_fileWrapper.Exists(filePath)) throw new FileNotFoundException(filePath);
            FilePath = filePath;
        }

        public int OnShow(int fShow)
        {
            var frameshow = (__FRAMESHOW) fShow;
            switch (frameshow)
            {
                case __FRAMESHOW.FRAMESHOW_WinClosed:
                    DeleteFile(FilePath);
                    break;
            }
            return VSConstants.S_OK;
        }

        private void DeleteFile(string filePath)
        {
            if(!_fileWrapper.Exists(FilePath)) return;
            _fileWrapper.Delete(FilePath);
        }

        public int OnMove()
        {
            return VSConstants.S_OK;
        }

        public int OnSize()
        {
            return VSConstants.S_OK;
        }

        public int OnDockableChange(int fDockable)
        {
            return VSConstants.S_OK;
        }

        public override string ToString()
        {
            return string.Format("FilePath: {0}", FilePath);
        }
    }
}