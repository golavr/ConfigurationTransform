using System;
using System.IO;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public sealed class StreamWriterWrapper : IStreamWriterWrapper, IDisposable
    {
        private readonly StreamWriter _streamWriter;

        public StreamWriterWrapper(string path, bool append)
        {
            _streamWriter = new StreamWriter(path, append);
        }

        public void Write(string value)
        {
            _streamWriter.Write(value);
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }
    }
}