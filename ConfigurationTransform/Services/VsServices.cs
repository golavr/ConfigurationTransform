using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
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

        public IVsWindowFrame OpenComparisonWindow2(string leftFileMoniker, string rightFileMoniker, string caption, string tooltip, string leftLabel, string rightLabel, string inlineLabel, string roles, uint grfDiffOptions)
        {
            // get diff service
            var diffService = Package.GetGlobalService(typeof(SVsDifferenceService)) as IVsDifferenceService;
            if (diffService == null) throw new NotSupportedException("IVsDifferenceService");

            var windowFrame = diffService.OpenComparisonWindow2(leftFileMoniker, rightFileMoniker, caption, tooltip, leftLabel, rightLabel,
                inlineLabel, roles, grfDiffOptions);

            return windowFrame;
        }

        public IVsWindowFrame OpenComparisonWindow(string leftFile, string rightFile,
            string caption, string tooltip,
            string leftLabel, string rightLabel)
        {
            const string inlineLabel = "{0}=>{1}";
            var windowFrame = OpenComparisonWindow2(leftFile, rightFile, caption, tooltip, leftLabel, rightLabel,
                inlineLabel, null, (uint)VsDiffOpt.RightFileIsTemporary);

            return windowFrame;
        }

        public virtual IVsWindowFrame OpenComparisonWindow(string leftFile, string rightFile,
            string leftLabel, string rightLabel)
        {
            var caption = string.Format("{0} vs. {1}", rightLabel, leftLabel);
            var tooltip = string.Format("Diff - {0}", rightLabel);
            var windowFrame = OpenComparisonWindow(leftFile, rightFile, caption, tooltip, leftLabel, rightLabel);

            return windowFrame;
        }

        public void OpenDiff(string leftFile, string rightFile, string leftLabel, string rightLabel)
        {
            try
            {
                // try to open diff within visual studio
                var windowFrame = OpenComparisonWindow(leftFile, rightFile, leftLabel, rightLabel);
                try
                {
                    windowFrame.TryRegisterCloseAndDeleteFile(rightFile);
                    return;
                }
                catch (Exception e)
                {
                    Trace.WriteLine(string.Format("Cannot register for file delete. File: {0}. Exception message: {1}", rightFile, e.Message));
                }
                return;
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format("Cannot open diff within visual studio. Exception message: {0}", e.Message));
            }
            var a = 1;
        }
    }
}