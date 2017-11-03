using System;
using System.Linq;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class ProjectExtensions
    {
        public static int GetProjectOutputType(this Project source)
        {
            return source.Properties.GetPropertyValue<int>("OutputType");
        }

        public static bool IsProjectOutputTypeExecutable(this Project source)
        {
            var projectOutputType = GetProjectOutputType(source);
            // project is a windows application or console application?
            return projectOutputType == 0 || projectOutputType == 1;
        }

        public static string[] GetBuildConfigurationNames(this Project source)
        {
            var configurationRowNames = source.ConfigurationManager.ConfigurationRowNames;
            var strings = ((Array)configurationRowNames).Cast<string>().ToArray();
            return strings;
        }

        public static void SaveReloadProject(this Project source)
        {
            var dte = source.DTE;

            if (!source.SelectProjectInExplorer()) return;

            dte.ExecuteCommand("File.SaveAll", string.Empty);
            // unload project
            dte.ExecuteCommand("Project.UnloadProject", string.Empty);
            // reload project
            dte.ExecuteCommand("Project.ReloadProject", string.Empty);
        }

        public static bool SelectProjectInExplorer(this Project source)
        {
            var dte = source.DTE;
            // Get the the Solution Explorer tree
            var solutionExplorer = ((UIHierarchy)dte.Windows.Item(Constants.vsWindowKindSolutionExplorer).Object);
            var solutionName = solutionExplorer.UIHierarchyItems.Item(1).Name;

            return SelectProject(solutionExplorer, solutionName, source.Name);
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
    }
}