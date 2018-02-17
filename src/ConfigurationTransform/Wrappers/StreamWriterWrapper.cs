using System;
using System.IO;
using System.Text;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public sealed class StreamWriterWrapper : IStreamWriterWrapper
    {
        private readonly StreamWriter _streamWriter;

        public StreamWriterWrapper(string path, bool append, Encoding encoding = null)
        {
            _streamWriter = new StreamWriter(path, append, encoding ?? Encoding.Default);
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