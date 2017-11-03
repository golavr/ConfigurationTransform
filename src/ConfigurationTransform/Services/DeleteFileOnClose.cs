using System;
using System.IO;
using GolanAvraham.ConfigurationTransform.Wrappers;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class DeleteFileOnClose : IDeleteFileOnClose
    {
        private readonly IFileWrapper _fileWrapper;
        public string FilePath { get; private set; }

        public DeleteFileOnClose(string filePath) : this(filePath, new FileWrapper())
        {
        }

        public DeleteFileOnClose(string filePath, IFileWrapper fileWrapper)
        {
            _fileWrapper = fileWrapper;
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException(filePath);
            if (!_fileWrapper.Exists(filePath)) throw new FileNotFoundException(filePath);
            FilePath = filePath;
        }

        public virtual void DeleteFile()
        {
            if (!_fileWrapper.Exists(FilePath)) return;
            _fileWrapper.Delete(FilePath);
        }

        public override string ToString()
        {
            return string.Format("FilePath: {0}", FilePath);
        }
    }
}