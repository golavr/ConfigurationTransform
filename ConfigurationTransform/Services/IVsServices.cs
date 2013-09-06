using EnvDTE;
using EnvDTE80;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public interface IVsServices
    {
        //DTE2 Dte { get; }
        //Project GetCurrentProject();
        //string GetProjectFileName(Project project);
        void ShowMessageBox(string title, string messageFormat, params object[] messageArgs);
        //string[] GetBuildConfigurationNames(Project project);
        //int GetProjectOutputType(Project project);
        //bool IsProjectOutputTypeExecutable(Project project);
        //string GetSelectedFileName();
        //ProjectItem GetSelectedItem();
        //bool IsLinkProjectItem(ProjectItem projectItem);
    }
}