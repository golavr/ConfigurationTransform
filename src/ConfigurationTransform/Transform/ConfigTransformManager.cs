using System;
using System.IO;
using System.Linq;
using System.Text;
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
<!-- In case configuration is not the root element, replace it with root element in source configuration file -->
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
            ProjectXmlTransform = new VsProjectXmlTransform(VsService);
            FileWrapper = new FileWrapper();
            StreamManager = new StreamManager();
        }

        //TODO:[Golan] - break this method to small pieces
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
                    // display yes/no message to user. yes - add as linked configs; no - add as concrete configs
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
                    VsService.OutputLine("Add LinkedConfigFiles");
                    // since it's link files we only need to copy them as like files to project
                    CreateLinkedConfigFiles(projectItem);
                }
                else
                {
                    VsService.OutputLine("Add ConfigFiles");
                    // create missing config files
                    CreateConfigFiles(project, projectItem);
                }

                // we need to know if we saved the file when displaying message to user
                var changed = project.IsDirty;
                // save before making external changes to file
                if (changed) project.Save();

                // project file(e.g. c:\myproject\myproject.csproj)
                var fileName = project.FullName;
                // config name (e.g. app.config or logging.config)
                var configName = projectItem.Name;

                ProjectXmlTransform.Open(fileName);
                VsService.OutputLine("Add UsingTask");
                ProjectXmlTransform.AddTransformTask();
                if (IsRootAppConfig(configName))
                {
                    VsService.OutputLine("Add AfterCompileTarget");
                    ProjectXmlTransform.AddAfterCompileTarget(configName, relativePrefix, createAsLinkedConfig);
                    VsService.OutputLine("Add AfterPublishTarget");
                    // project is a windows application or console application? if so add click once transform task
                    ProjectXmlTransform.AddAfterPublishTarget(configName, relativePrefix, createAsLinkedConfig);
                }
                else
                {
                    VsService.OutputLine("Add AfterBuildTarget");
                    ProjectXmlTransform.AddAfterBuildTarget(configName, relativePrefix, createAsLinkedConfig);
                }

                VsService.OutputLine("Check if need to save project file");
                // save project file
                var isSaved = ProjectXmlTransform.Save();
                // check if need to reload project, remember that we edit the project file externally
                if (isSaved)
                {
                    VsService.OutputLine("Done saving project file");
                    VsService.OutputLine("Reloading project file");
                    project.SaveReloadProject();
                }
                else
                {
                    VsService.OutputLine("No changes made in project file");
                }
                return changed || isSaved;
            }
            finally
            {
                dte.SuppressUI = false;
            }
        }

        public static void CreateLinkedConfigFiles(ProjectItem targetProjectItem)
        {
            // get source root config project item
            var sourceRootConfig = targetProjectItem.GetProjectItemContainingFullPath();

            // source config is not included in project
            if (sourceRootConfig == null)
            {
                CreateLikedConfigFilesNotFromProject(targetProjectItem);
            }
            else
            {
                if (sourceRootConfig.ProjectItems == null) return;

                CreateLikedConfigFilesFromProject(targetProjectItem, sourceRootConfig);
            }
        }

        private static void CreateLikedConfigFilesFromProject(ProjectItem targetProjectItem, ProjectItem sourceRootConfig)
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

        private static void CreateLikedConfigFilesNotFromProject(ProjectItem targetProjectItem)
        {
            var configName = targetProjectItem.Name;
            var sourceConfigPath = targetProjectItem.GetFullPath();
            var project = targetProjectItem.ContainingProject;
            var buildConfigurationNames = project.GetBuildConfigurationNames();
            
            var sourceConfigDirectory = Directory.GetParent(sourceConfigPath).FullName;

            foreach (var buildConfigurationName in buildConfigurationNames)
            {
                var dependentConfig = GetTransformConfigName(configName, buildConfigurationName);
                var sourceDependentConfigFullPath = Path.Combine(sourceConfigDirectory, dependentConfig);
                // check if source config file exist and not exist in target
                if (FileWrapper.Exists(sourceDependentConfigFullPath) &&
                    targetProjectItem.ProjectItems.AsEnumerable().All(c => c.Name != dependentConfig))
                {
                    targetProjectItem.ProjectItems.AddFromFile(sourceDependentConfigFullPath);
                }
            }
        }


        // disclaimer: visual studio doesn't support adding dependent file under linked file
        // so no dependent transformed config under linked app.config in designer
        private static void CreateConfigFiles(Project project, ProjectItem projectItem)
        {
            var appConfigName = projectItem.Name;
            var buildConfigurationNames = project.GetBuildConfigurationNames();
            // get app.config directory. new transform config will be created there.
            string path;
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
                if (FileWrapper.Exists(dependentConfigFullPath))
                {
                    VsService.OutputLine($"File {dependentConfig} already exists");
                    continue;
                }

                using (var file = FileWrapper.AppendText(dependentConfigFullPath))
                {
                    file.Write(DependencyConfigContent);
                }
                projectItem.ProjectItems.AddFromFile(dependentConfigFullPath);
                VsService.OutputLine($"File {dependentConfig} was added");
            }
        }

        public static string GetTransformConfigName(string sourceConfigName, string buildConfigurationName)
        {
            var configSplit = sourceConfigName.Split('.');
            if (configSplit.Length < 2) throw new NotSupportedException(sourceConfigName);
            var dependentConfig = $"{configSplit[0]}.{buildConfigurationName}.{configSplit[1]}";
            return dependentConfig;
        }

        public static bool IsTransformConfigName(string configName)
        {
            var configSplit = configName.Split('.');
            if (configSplit.Length < 3) return false;
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
            //catch (XmlException ex)
            //{
            //    VsService.OutputLine($"Failed to open {sourceFile}");
            //    throw;
            //}
            catch (Exception e)
            {
                VsService.OutputLine($"Failed to open {sourceFile}");
                throw;
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
            using (var file = StreamManager.NewStreamWriter(tempFilePath, false, Encoding.UTF8))
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