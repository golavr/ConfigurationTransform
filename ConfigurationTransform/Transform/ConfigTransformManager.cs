using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using Microsoft.VisualStudio.Shell.Interop;
using Constants = EnvDTE.Constants;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public class ConfigTransformManager
    {
        private const string DependencyConfigContent =
@"<?xml version=""1.0""?>
<!-- For more information on using app.config transformation visit http://go.microsoft.com/fwlink/?LinkId=125889 -->
<configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
</configuration>";

        /// <summary>
        /// replaceable vs service for testing.
        /// </summary>
        public static IVsServices VsService { get; set; }

        public static IVsProjectXmlTransform ProjectXmlTransform { get; set; }

        static ConfigTransformManager()
        {
            // add default vs service
            VsService = VsServices.Instance;
            ProjectXmlTransform = new VsProjectXmlTransform();
        }

        public static bool EditProjectFile(ProjectItem projectItem)
        {
            var appConfigName = projectItem.Name;
            string relativePrefix = null;

            // get dte from project item
            var dte = projectItem.DTE;
            try
            {
                // hide UI changes
                dte.SuppressUI = true;

                var isLinkAppConfig = projectItem.IsLink();

                var project = projectItem.ContainingProject;
                // check if its linked config
                if (isLinkAppConfig)
                {
                    // display yes/no message to user. yes - add as lined configs; no - add as concrete configs
                    var result = VsService.ShowMessageBox("Add as linked conifg?",
                                                          OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                                          OLEMSGICON.OLEMSGICON_QUERY);
                    if (result == 6)
                    {
                        // since it's link files we only need to copy them as like files to project
                        CreateLinkedAppConfigFiles(projectItem);
                        relativePrefix = projectItem.GetRelativeDirectory();
                    }
                    else
                    {
                        // create missing config files
                        CreateAppConfigFiles(project, projectItem, appConfigName);
                    }
                }
                else    
                {
                    // create missing config files
                    CreateAppConfigFiles(project, projectItem, appConfigName);
                }

                var fileName = project.FullName;
                ProjectXmlTransform.Open(fileName);
                //var vsProjectXml = new VsProjectXmlTransform(fileName);

                ProjectXmlTransform.AddTransformTask();
                ProjectXmlTransform.AddAfterCompileTarget(appConfigName, relativePrefix);
                // project is a windows application or console application? if so add click once transform task
                if (project.IsProjectOutputTypeExecutable())
                {
                    ProjectXmlTransform.AddAfterPublishTarget(appConfigName);
                }

                // save project file
                var isSaved = ProjectXmlTransform.Save();
                // check if need to reload project
                if (isSaved)
                {
                    ReloadProject(project);
                }
                return isSaved;
            }
            finally
            {
                dte.SuppressUI = false;
            }
        }

        public static void CreateLinkedAppConfigFiles(ProjectItem targetProjectItem)
        {
            // get source root config project item
            var sourceRootConfig = targetProjectItem.GetProjectItemContainingFullPath();

            if (sourceRootConfig.ProjectItems == null) return;
            // iterate source root config items
            foreach (var item in sourceRootConfig.ProjectItems.AsEnumerable())
            {
                // get source config dependent config file name
                var sourceFullPath = item.GetFullPath();
                // get target root config not contains source dependent config?
                if (targetProjectItem.ProjectItems.AsEnumerable().All(
                    projectItem => projectItem.GetFullPath() != sourceFullPath))
                {
                    // add dependent config to target root config
                    targetProjectItem.ProjectItems.AddFromFile(sourceFullPath);
                }
            }
            // get item containing project
            var targetProject = targetProjectItem.ContainingProject;
            // save target project file
            if (targetProject.IsDirty) targetProject.Save();
        }

        private static void ReloadProject(Project project)
        {
            var dte = project.DTE;

            if (!SelectProjectInExplorer(project)) return;

            dte.ExecuteCommand("File.SaveAll", string.Empty);
            // unload project
            dte.ExecuteCommand("Project.UnloadProject", string.Empty);
            // reload project
            dte.ExecuteCommand("Project.ReloadProject", string.Empty);
        }

        private static bool SelectProjectInExplorer(Project project)
        {
            var dte = project.DTE;
            // Get the the Solution Explorer tree
            var solutionExplorer = ((UIHierarchy)dte.Windows.Item(Constants.vsWindowKindSolutionExplorer).Object);
            var solutionName = solutionExplorer.UIHierarchyItems.Item(1).Name;

            return SelectProject(solutionExplorer, solutionName, project.Name);
        }

        private static bool SelectProject(UIHierarchy solutionExplorer, string solutionName, string projectName)
        {
            var selectedItems = solutionExplorer.SelectedItems as Array;
            if (selectedItems == null) return false;
            var selectedItem = selectedItems.GetValue(0) as UIHierarchyItem;

            try
            {
                // is it the project we are looking for?
                if (selectedItem.Name == projectName) return true;
                // is it the root?
                if (selectedItem.Name == solutionName) return false;
            }
                // suppress exceptions for items w/o names
                // ReSharper disable EmptyGeneralCatchClause
            catch
                // ReSharper restore EmptyGeneralCatchClause
            {
            }
            // move up in hierarchy
            solutionExplorer.SelectUp(vsUISelectionType.vsUISelectionTypeSelect, 1);
            return SelectProject(solutionExplorer, solutionName, projectName);
        }

        public static bool IsRootAppConfig(string fileName)
        {
            if (!fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)) return false;
            if (fileName.Split('.').Length > 2) return false;
            return true;
        }

        private static void CreateAppConfigFiles(Project project, ProjectItem projectItem, string appConfigName)//, IEnumerable<string> buildConfigurationNames)//, string path)
        {
            var buildConfigurationNames = project.GetBuildConfigurationNames();
            var projectFileIsDirty = false;
            // get app.config directory. new transform config will be created there.
            //var path = projectItem.GetFullPath();
            var path = Directory.GetParent(projectItem.GetFullPath()).FullName;

            foreach (var buildConfigurationName in buildConfigurationNames)
            {
                var dependentConfig = GetTransformConfigName(appConfigName, buildConfigurationName);
                var dependentConfigFullPath = Path.Combine(path, dependentConfig);
                // check if config file exist
                if (!File.Exists(dependentConfigFullPath))
                {
                    using (var file = File.AppendText(dependentConfigFullPath))
                    {
                        file.Write(DependencyConfigContent);
                    }
                    projectFileIsDirty = true;
                    projectItem.ProjectItems.AddFromFile(dependentConfigFullPath);
                    //project.ProjectItems.AddFromFile(dependentConfigFullPath);
                }
            }
            // save project file in case we changed it
            if (projectFileIsDirty)
            {
                project.Save();
            }
        }

        public static string GetTransformConfigName(string sourceConfigName, string buildConfigurationName)
        {
            var appConfigSplit = sourceConfigName.Split('.');
            if (appConfigSplit.Length < 2) throw new NotSupportedException(sourceConfigName);
            var dependentConfig = string.Format("{0}.{1}.{2}", appConfigSplit[0], buildConfigurationName,
                                                    appConfigSplit[1]);
            return dependentConfig;
        }

        public static bool IsTransformConfigName(string configName)
        {
            var appConfigSplit = configName.Split('.');
            if (appConfigSplit.Length < 3) return false;
            if (!configName.EndsWith(".config")) return false;

            return true;
        }
    }
}