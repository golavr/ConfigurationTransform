using System;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public interface IStreamManager
    {
        IStreamWriterWrapper NewStreamWriter(string path, bool append);
    }
}