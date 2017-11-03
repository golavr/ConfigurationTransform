using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class ProjectItemsExtensions
    {
        public static IEnumerable<ProjectItem> AsEnumerable(this ProjectItems source)
        {
            return source.Cast<ProjectItem>();
        }

        public static bool IsProjectItemPropertiesIncluded(this ProjectItems source, Predicate<IEnumerable<Property>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            foreach (var projectItem in source.AsEnumerable())
            {
                if (projectItem.ProjectItems.Count > 0)
                {
                    if (projectItem.ProjectItems.IsProjectItemPropertiesIncluded(predicate)) return true;
                }

                var isProjectItemPropertiesIncluded = predicate.Invoke(projectItem.Properties.AsEnumerable());
                if (isProjectItemPropertiesIncluded) return true;
            }
            return false;
        }

        public static ProjectItem GetProjectItemHavingProperties(this ProjectItems source, Predicate<IEnumerable<Property>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            foreach (var projectItem in source.AsEnumerable())
            {
                // check current leaf
                var isProjectItemPropertiesIncluded = projectItem.IsHavingProperties(predicate);//predicate.Invoke(projectItem.Properties.AsEnumerable());
                if (isProjectItemPropertiesIncluded) return projectItem;

                // check if branch
                if (projectItem.ProjectItems.Count > 0)
                {
                    // call self
                    var projectItemPropertiesIncluded = projectItem.ProjectItems.GetProjectItemHavingProperties(predicate);
                    if (projectItemPropertiesIncluded != null) return projectItemPropertiesIncluded;
                }
            }
            return null;
        }

        public static ProjectItem GetProjectItemHavingProperties(this ProjectItems source, Predicate<Properties> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }

            foreach (var projectItem in source.AsEnumerable())
            {
                // check current leaf
                var isProjectItemPropertiesIncluded = projectItem.IsHavingProperties(predicate);//predicate.Invoke(projectItem.Properties.AsEnumerable());
                if (isProjectItemPropertiesIncluded) return projectItem;

                if (projectItem.ProjectItems == null)
                {
                    if (projectItem.SubProject == null || projectItem.SubProject.ProjectItems == null) continue;
                    var subProjectItemHavingProperties = projectItem.SubProject.ProjectItems.GetProjectItemHavingProperties(predicate);
                    if (subProjectItemHavingProperties != null) return subProjectItemHavingProperties;
                    continue;
                }
                // check if branch
                if (projectItem.ProjectItems.AsEnumerable().Any())
                {
                    // call self
                    var projectItemPropertiesIncluded = projectItem.ProjectItems.GetProjectItemHavingProperties(predicate);
                    if (projectItemPropertiesIncluded != null) return projectItemPropertiesIncluded;
                }
            }
            return null;
        }
    }
}