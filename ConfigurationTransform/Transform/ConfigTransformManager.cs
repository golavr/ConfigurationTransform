using System;
using System.IO;
using System.Linq;
using System.Xml;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using GolanAvraham.ConfigurationTransform.Services.Helpers;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Web.XmlTransform;

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

        public static IFileWrapper FileWrapper { get; set; }
        public static IStreamManager StreamManager { get; set; }

        static ConfigTransformManager()
        {
            // add default vs service
            VsService = VsServices.Instance;
            ProjectXmlTransform = new VsProjectXmlTransform();
            FileWrapper = new FileWrapper();
            StreamManager = new StreamManager();
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
                    var result = VsService.ShowMessageBox("Add as linked configs?",
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

            // sorce config is not included in project
            if (sourceRootConfig == null)
            {
                CreateLikedAppConfigFilesNotFromProject(targetProjectItem);
            }
            else
            {
                if (sourceRootConfig.ProjectItems == null) return;

                CreateLikedAppConfigFilesFromProject(targetProjectItem, sourceRootConfig);
            }

            // get item containing project
            var targetProject = targetProjectItem.ContainingProject;
            // save target project file
            if (targetProject.IsDirty) targetProject.Save();
        }

        private static void CreateLikedAppConfigFilesFromProject(ProjectItem targetProjectItem, ProjectItem sourceRootConfig)
        {
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
        }

        private static void CreateLikedAppConfigFilesNotFromProject(ProjectItem targetProjectItem)
        {
            var appConfigName = targetProjectItem.Name;
            var sourceAppConfigPath = targetProjectItem.GetFullPath();
            var project = targetProjectItem.ContainingProject;
            var buildConfigurationNames = project.GetBuildConfigurationNames();
            
            var sourceConfigDirectory = Directory.GetParent(sourceAppConfigPath).FullName;

            foreach (var buildConfigurationName in buildConfigurationNames)
            {
                var dependentConfig = GetTransformConfigName(appConfigName, buildConfigurationName);
                var sourceDependentConfigFullPath = Path.Combine(sourceConfigDirectory, dependentConfig);
                // check if source config file exist and not exist in target
                if (FileWrapper.Exists(sourceDependentConfigFullPath) &&
                    targetProjectItem.ProjectItems.AsEnumerable().All(c => c.Name != dependentConfig))
                {
                    targetProjectItem.ProjectItems.AddFromFile(sourceDependentConfigFullPath);
                }
            }
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
                if (!FileWrapper.Exists(dependentConfigFullPath))
                {
                    using (var file = FileWrapper.AppendText(dependentConfigFullPath))
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
            if (!fileName.StartsWith("app", StringComparison.OrdinalIgnoreCase)) return false;
            return IsRootConfig(fileName);
        }

        public static bool IsRootConfig(string fileName)
        {
            if (!fileName.EndsWith(".config", StringComparison.OrdinalIgnoreCase)) return false;
            if (fileName.Split('.').Length > 2) return false;
            return true;
        }

        /// <summary>
        /// Return transformed xml string 
        /// </summary>
        /// <param name="sourcePath">app.config</param>
        /// <param name="targetPath">app.debug.config</param>
        /// <returns>Transformed xml string</returns>
        public static string GetTransformString(string sourcePath, string targetPath)
        {
            var xmlDocument = OpenSourceFile(sourcePath);

            var xmlTransformation = new XmlTransformation(targetPath);
            xmlTransformation.Apply(xmlDocument);

            var xmlString = xmlDocument.OuterXml;
            return xmlString;
        }

        private static XmlTransformableDocument OpenSourceFile(string sourceFile)
        {
            try
            {
                XmlTransformableDocument transformableDocument = new XmlTransformableDocument();
                transformableDocument.PreserveWhitespace = true;
                transformableDocument.Load(sourceFile);
                return transformableDocument;
            }
            catch (XmlException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }

        public static void PreviewTransform(ProjectItem projectItem)
        {
            var parent = projectItem.ParentProjectItemOrDefault();
            if (parent == null)
            {
                VsServices.Instance.ShowMessageBox("Cannot find source config");
                return;
            }
            var targetFileName = projectItem.Name;
            var sourceLabel = parent.Name;
            var sourcePath = parent.GetFullPath();
            var targetLabel = targetFileName;

            // apply transform on file
            var xmlString = GetTransformString(sourcePath, projectItem.GetFullPath());
            // create temp file
            var tempFilePath = GetTempFilePath(targetFileName);

            var targetPath = tempFilePath;
            // write to temp file
            WriteToFile(tempFilePath, xmlString);

            VsService.OpenDiff(sourcePath, targetPath, sourceLabel, targetLabel);
        }

        private static void WriteToFile(string tempFilePath, string stringToWrite)
        {
            using (var file = StreamManager.NewStreamWriter(tempFilePath, false))
            {
                file.Write(stringToWrite);
            }
        }

        private static string GetTempFilePath(string fileName)
        {
            var tempPath = Path.GetTempPath();
            var tempFileName = PathHelper.AppendToFileName(fileName, string.Format("_{0}", Guid.NewGuid()));
            var tempFilePath = Path.Combine(tempPath, tempFileName);
            return tempFilePath;
        }




    }
}