using System;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public interface IStreamWriterWrapper : IDisposable
    {
        void Write(string value);
    }
}