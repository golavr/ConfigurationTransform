namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public class StreamManager : IStreamManager
    {
        public IStreamWriterWrapper NewStreamWriter(string path, bool append)
        {
            return new StreamWriterWrapper(path, append);
        }
    }
}