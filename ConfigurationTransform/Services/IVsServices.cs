using EnvDTE;
using EnvDTE80;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IVsServices
    {
        void ShowMessageBox(string title, string messageFormat, params object[] messageArgs);
    }
}