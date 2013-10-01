using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IVsServices
    {
        //void ShowMessageBox(string title, string messageFormat, params object[] messageArgs);
        int ShowMessageBox(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO);
    }
}