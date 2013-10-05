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

        public int ShowMessageBox(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
        {
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
                       buttons,
                       OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                       icon,
                       0,        // false
                       out result));
            return result;
        }
    }
}