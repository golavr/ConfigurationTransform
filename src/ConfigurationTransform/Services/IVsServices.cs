using EnvDTE;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IVsServices
    {
        //void ShowMessageBox(string title, string messageFormat, params object[] messageArgs);
        int ShowMessageBox(string title, string message, OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO);

        //
        // Summary:
        //     Opens and displays a file comparison window in Visual Studio.
        //
        // Parameters:
        //   leftFileMoniker:
        //     [in] Path to the file that will be displayed in the left side of the comparison.
        //
        //   rightFileMoniker:
        //     [in] Path to the file that will be displayed in the right side of the comparison.
        //
        //   caption:
        //     [in] Caption to display in the document tab. If this parameter is null or
        //     empty, {0} vs. {1} is shown.
        //
        //   Tooltip:
        //     [in] Tooltip to display for the document tab. If this parameter is null or
        //     empty, the default tooltip is used.
        //
        //   leftLabel:
        //     [in] Label to display above the left view. If this parameter is null or empty,
        //     then no label is shown.
        //
        //   rightLabel:
        //     [in] Label to display above the right view. If this parameter is null or
        //     empty, then no label is shown.
        //
        //   inlineLabel:
        //     [in] Label to display above the inline view. If this parameter is null or
        //     empty, then no label is shown.
        //
        //   roles:
        //     [in] Additional text view roles added to the difference views.
        //
        //   grfDiffOptions:
        //     [in] Mask of options for the comparison window.
        //
        // Returns:
        //     Window frame for the comparison view.
        IVsWindowFrame OpenComparisonWindow2(string leftFileMoniker, string rightFileMoniker, string caption, string tooltip, string leftLabel, string rightLabel, string inlineLabel, string roles, uint grfDiffOptions);
        void OpenDiff(string leftFile, string rightFile, string leftLabel, string rightLabel);
        void OutputLine(string message);
    }
}