using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.Shell;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class DTEExtensions
    {
        public static DTE GetInstance()
        {
            return (DTE)Package.GetGlobalService(typeof(DTE));
        }

        public static SelectedItem GetSelectedItem(this DTE dte)
        {
            return dte.SelectedItems.AsEnumerable().Single();
        }

        public static Project FindProjectByProjectItemProperties(this DTE dte,
                                                                 Predicate<IEnumerable<Property>> predicate)
        {
            var projects = dte.Solution.Projects.AsEnumerable();
            var project =
                projects.FirstOrDefault(project1 => project1.ProjectItems.IsProjectItemPropertiesIncluded(predicate));
            return project;
        }

        public static ProjectItem GetProjectItemHavingProperties(this DTE dte,
                                                                 Predicate<IEnumerable<Property>> predicate)
        {
            foreach (
                var projectItem in
                    dte.Solution.Projects.AsEnumerable().SelectMany(project => project.ProjectItems.AsEnumerable()))
            {
                if (projectItem.IsHavingProperties(predicate)) return projectItem;
                var projectItemHavingProperties = projectItem.ProjectItems.GetProjectItemHavingProperties(predicate);
                if (projectItemHavingProperties != null) return projectItemHavingProperties;
            }
            return null;
        }

        public static ProjectItem GetProjectItemHavingProperties(this DTE dte,
                                                         Predicate<Properties> predicate)
        {
            foreach (
                var projectItem in
                    dte.Solution.Projects.AsEnumerable().SelectMany(project => project.ProjectItems.AsEnumerable()))
            {
                if (projectItem.IsHavingProperties(predicate)) return projectItem;
                var projectItemHavingProperties = projectItem.ProjectItems.GetProjectItemHavingProperties(predicate);
                if (projectItemHavingProperties != null) return projectItemHavingProperties;
            }
            return null;
        }

        //TODO:[Golan] - remove
        //public static ProjectItem GetProjectItemHavingProperties(this DTE2 dte,
        //                                                         Predicate<FileProperties2> predicate)
        //{
        //    var itemHavingProperties =
        //        dte.Solution.Projects.AsEnumerable()
        //           .Select(project => project.ProjectItems)
        //           .Select(projectItems => projectItems.GetProjectItemHavingProperties(predicate))
        //           .FirstOrDefault(projectItemHavingProperties => projectItemHavingProperties != null);

        //    return itemHavingProperties;
        //}
    }
}