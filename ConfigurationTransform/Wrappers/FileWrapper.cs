using System.IO;
using GolanAvraham.ConfigurationTransform.Transform;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public class FileWrapper : IFileWrapper
    {
        public StreamWriter AppendText(string path)
        {
            return File.AppendText(path);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }
    }
}