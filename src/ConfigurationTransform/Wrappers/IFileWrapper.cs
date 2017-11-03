using System.IO;

namespace GolanAvraham.ConfigurationTransform.Wrappers
{
    public interface IFileWrapper
    {
        StreamWriter AppendText(string path);
        bool Exists(string path);
        void Delete(string path);
    }
}