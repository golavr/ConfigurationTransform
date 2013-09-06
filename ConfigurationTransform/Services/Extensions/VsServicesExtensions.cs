namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class VsServicesExtensions
    {
        public static void ShowMessageBox(this IVsServices source, string messageFormat, params object[] messageArgs)
        {
            source.ShowMessageBox("ConfigurationTransform", messageFormat, messageArgs);
        }
    }
}