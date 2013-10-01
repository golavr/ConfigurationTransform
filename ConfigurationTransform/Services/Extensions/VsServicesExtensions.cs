using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class VsServicesExtensions
    {
        public static int ShowMessageBox(this IVsServices source, string message,
                                         OLEMSGBUTTON buttons = OLEMSGBUTTON.OLEMSGBUTTON_OK,
                                         OLEMSGICON icon = OLEMSGICON.OLEMSGICON_INFO)
        {
            var result = source.ShowMessageBox("ConfigurationTransform", message, buttons, icon);
            return result;
        }
    }
}