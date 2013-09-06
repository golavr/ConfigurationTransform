using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using EnvDTE80;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using GolanAvraham.ConfigurationTransform.Services.Implementations;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Services
{
    public class VsServices : IVsServices
    {
        private static readonly VsServices _instance = new VsServices();

        //private readonly DTE2 _dte = (DTE2)Package.GetGlobalService(typeof(DTE));

        //public virtual DTE2 Dte { get { return _dte; } }

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

        //public Project GetCurrentProject()
        //{
        //    Project currentProject = null;
        //    var projects = (Array)Dte.ActiveSolutionProjects;
        //    if (projects.Length > 0)
        //    {
        //        currentProject = projects.GetValue(0) as Project;
        //    }

        //    return currentProject;
        //}

        //public string GetProjectFileName(Project project)
        //{
        //    return project.FullName;
        //}

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

        //public string[] GetBuildConfigurationNames(Project project)
        //{
        //    var configurationRowNames = project.ConfigurationManager.ConfigurationRowNames;
        //    var strings = ((Array)configurationRowNames).Cast<string>().ToArray();
        //    return strings;
        //}

        //public int GetProjectOutputType(Project project)
        //{
        //    return (int)project.Properties.Item("OutputType").Value;
        //}

        //public bool IsProjectOutputTypeExecutable(Project project)
        //{
        //    var projectOutputType = GetProjectOutputType(project);
        //    // project is a windows application or console application?
        //    return projectOutputType == 0 || projectOutputType == 1;
        //}

        //public ProjectItem GetSelectedItem()
        //{
        //    return Dte.GetSelectedItem();
        //}

        //TODO:[Golan] - UT GetProjectContainingProjectItemFullPath
        //public Project GetProjectContainingProjectItemFullPath(string projectItemFullPath, bool isLink = false)
        //{
        //    return
        //        Dte.FindProjectByProjectItemProperties(
        //            b => b.Count(property =>
        //                         (property.Name == "FullPath" && property.Value.ToString() == projectItemFullPath) ||
        //                         (property.Name == "IsLink" && property.Value.ToString() == isLink.ToString())) == 2);
        //}

        //TODO:[Golan] - UT GetProjectItemContainingFullPath
        //public ProjectItem GetProjectItemContainingFullPath(string projectItemFullPath, bool isLink = false)
        //{


        //    return
        //        Dte.GetProjectItemHavingProperties(
        //            b => b.Count(property =>
        //                         (property.Name == "FullPath" && property.Value.ToString() == projectItemFullPath) ||
        //                         (property.Name == "IsLink" && property.Value.ToString() == isLink.ToString())) == 2);
        //}

        //public ProjectItem GetProjectItemContainingFullPath(string projectItemFullPath, bool isLink = false)
        //{
        //    return
        //        Dte.GetProjectItemHavingProperties(properties => 
        //            properties.GetFullPath() == projectItemFullPath && 
        //            properties.GetIsLink() == isLink);

        //}
    }
}