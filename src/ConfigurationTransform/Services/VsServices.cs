using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class VsServices : IVsServices
    {
        private static readonly VsServices _instance = new VsServices();

        public static VsServices Instance => _instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static VsServices()
        {
        }

        protected VsServices()
        {
        }

        public void OutputLine(string message)
        {
            // Get the output window
            var outputWindow = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;

            // Ensure that the desired pane is visible
            var paneGuid = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;
            IVsOutputWindowPane pane;
            outputWindow.CreatePane(paneGuid, "ConfigurationTransform", 1, 0);
            outputWindow.GetPane(paneGuid, out pane);

            // Output the message
            pane.OutputString($"{message}{Environment.NewLine}");
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
            if (!(Package.GetGlobalService(typeof(SVsDifferenceService)) is IVsDifferenceService diffService))
                throw new NotSupportedException("IVsDifferenceService");

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
            var caption = $"{rightLabel} vs. {leftLabel}";
            var tooltip = $"Diff - {rightLabel}";
            var windowFrame = OpenComparisonWindow(leftFile, rightFile, caption, tooltip, leftLabel, rightLabel);

            return windowFrame;
        }

        public void OpenDiff(string leftFile, string rightFile, string leftLabel, string rightLabel)
        {
            // first try to compare
            if (CompareFilesDeleteOnClose(leftFile, rightFile, leftLabel, rightLabel)) return;
            OutputLine("Open transformed file as fallback solution");
            // fallback call for document open
            var dte2 = DTEExtensions.GetInstance();
            OpenFileDeleteOnClose(rightFile, dte2);
        }

        private bool CompareFilesDeleteOnClose(string leftFile, string rightFile, string leftLabel, string rightLabel)
        {
            try
            {
                // try to open diff within visual studio
                var windowFrame = OpenComparisonWindow(leftFile, rightFile, leftLabel, rightLabel);
                try
                {
                    windowFrame.TryRegisterCloseAndDeleteFile(rightFile);
                    return true;
                }
                catch (Exception e)
                {
                    var message = $"Cannot register for file delete. File: {rightFile}. Exception message: {e.Message}";
                    Trace.WriteLine(message);
                    OutputLine(message);
                }
                return true;
            }
            catch (Exception e)
            {
                var message = $"Cannot open diff within visual studio. Exception message: {e.Message}";
                Trace.WriteLine(message);
                OutputLine(message);
            }
            return false;
        }

        private bool OpenFileDeleteOnClose(string path, DTE dte2)
        {
            try
            {
                dte2.Documents.Open(path, "Auto", true);
                try
                {
                    path.TryRegisterCloseAndDeleteFile();
                }
                catch (Exception e)
                {
                    var message = $"Cannot register for file delete. File: {path}. Exception message: {e.Message}";
                    Trace.WriteLine(message);
                    OutputLine(message);
                }
                return true;
            }
            catch (Exception e)
            {
                var message = $"Cannot open file within visual studio. Exception message: {e.Message}";
                Trace.WriteLine(message);
                OutputLine(message);
            }
            return false;
        }
    }
}