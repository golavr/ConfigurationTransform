using System.IO;

namespace GolanAvraham.ConfigurationTransform.Services.Helpers
{
    public static class PathHelper
    {
        public static string AppendToFileName(string fileName, string append)
        {
            var prefix = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName);
            var newFileName = string.Format("{0}{1}{2}", prefix, append, extension);
            return newFileName;
        }
    }
}