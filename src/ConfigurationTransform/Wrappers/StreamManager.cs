using System.Text;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public class StreamManager : IStreamManager
    {
        public IStreamWriterWrapper NewStreamWriter(string path, bool append, Encoding encoding = null)
        {
            return new StreamWriterWrapper(path, append, encoding);
        }
    }
}