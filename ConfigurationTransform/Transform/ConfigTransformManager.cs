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
            string relativePrefix = null;

            // get dte from project item
            var dte = projectItem.DTE;
            try
            {
                // hide UI changes
                dte.SuppressUI = true;

                var isLinkAppConfig = projectItem.IsLink();

                var project = projectItem.ContainingProject;
                var createAsLinkedConfig = false;
                // check if its linked config
                if (isLinkAppConfig)
                {
                    // display yes/no message to user. yes - add as lined configs; no - add as concrete configs
                    var result = VsService.ShowMessageBox("Add as linked conifgs?",
                                                          OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                                          OLEMSGICON.OLEMSGICON_QUERY);

                    // store relative path
                    relativePrefix = projectItem.GetRelativeDirectory();
                    if (result == MessageBoxResult.Yes)
                    {
                        createAsLinkedConfig = true;
                    }
                }

                if (createAsLinkedConfig)
                {
                    // since it's link files we only need to copy them as like files to project
                    CreateLinkedAppConfigFiles(projectItem);
                }
                else
                {
                    // create missing config files
                    CreateAppConfigFiles(project, projectItem);
                }

                // project file(e.g. c:\myproject\myproject.csproj)
                var fileName = project.FullName;
                // app config name (e.g. app.config)
                var appConfigName = projectItem.Name;

                ProjectXmlTransform.Open(fileName);
                ProjectXmlTransform.AddTransformTask();
                ProjectXmlTransform.AddAfterCompileTarget(appConfigName, relativePrefix, createAsLinkedConfig);
                // project is a windows application or console application? if so add click once transform task
                // removed this check for deployed class library projects (Word Add-In)
                //if (project.IsProjectOutputTypeExecutable())
                //{
                ProjectXmlTransform.AddAfterPublishTarget();
                //}

                // save project file
                var isSaved = ProjectXmlTransform.Save();
                // check if need to reload project
                if (isSaved)
                {
                    project.SaveReloadProject();
                    //ReloadProject(project);
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

        // disclaimer: visual studio don't support adding dependent file under linked file
        // so no dependent transformed config under linked app.config in designer
        private static void CreateAppConfigFiles(Project project, ProjectItem projectItem)
        {
            string appConfigName = projectItem.Name;
            var buildConfigurationNames = project.GetBuildConfigurationNames();
            var projectFileIsDirty = false;
            // get app.config directory. new transform config will be created there.
            //var path = projectItem.GetFullPath();
            string path = null;
            if (projectItem.IsLink())
            {
                path = Directory.GetParent(projectItem.ContainingProject.FullName).FullName;
            }
            else
            {
                path = Directory.GetParent(projectItem.GetFullPath()).FullName;
            }

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

        public static bool IsRootAppConfig(string fileName)
        {
            if (!fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)) return false;
            if (fileName.Split('.').Length > 2) return false;
            return true;
        }
    }
}