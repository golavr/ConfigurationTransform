using System;
using System.Text;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public interface IStreamManager
    {
        IStreamWriterWrapper NewStreamWriter(string path, bool append, Encoding encoding = null);
    }
}