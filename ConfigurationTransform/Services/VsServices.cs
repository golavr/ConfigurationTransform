using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class VsServices : IVsServices
    {
        private static readonly VsServices _instance = new VsServices();

        public static VsServices Instance
        {
            get { return _instance; }
        }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static VsServices()
        {
        }

        protected VsServices()
        {
        }

        public void ShowMessageBox(string title, string messageFormat, params object[] messageArgs)
        {
            var message = string.Format(messageFormat, messageArgs);
            var uiShell = (IVsUIShell)Package.GetGlobalService(typeof(SVsUIShell));
            var clsid = Guid.Empty;
            int result;
            ErrorHandler.ThrowOnFailure(uiShell.ShowMessageBox(
                       0,
                       ref clsid,
                       title,
                       message,
                       string.Empty,
                       0,
                       OLEMSGBUTTON.OLEMSGBUTTON_OK,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       OLEMSGICON.OLEMSGICON_INFO,
                       0,        // false
                       out result));
        }
    }
}