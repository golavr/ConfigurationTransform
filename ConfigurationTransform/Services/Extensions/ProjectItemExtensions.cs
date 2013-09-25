using System;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using EnvDTE80;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class ProjectItemExtensions
    {
        public static bool TryGetPropertyValue<TResult>(this ProjectItem source, string propertyName,
                                                        out TResult propertyValue)
        {
            propertyValue = default(TResult);
            var property = source.Properties.AsEnumerable().SingleOrDefault(s => s.Name == propertyName);
            if (property == null)
            {
                return false;
            }
            propertyValue = (TResult)property.Value;
            return true;
        }


        public static TResult GetPropertyValue<TResult>(this ProjectItem source, string propertyName)
        {
            var propertyValue = source.Properties.AsEnumerable().Where(s => s.Name == propertyName).Select(s=>s.Value).Single();
            return (TResult) propertyValue;
        }

        public static string GetFullPath(this ProjectItem source)
        {
            const string fullPath = "FullPath";
            var propertyValue = source.GetPropertyValue<string>(fullPath);
            return propertyValue;
        }

        public static bool IsLink(this ProjectItem source)
        {
            // is link file?
            bool isLink;
            return (source.TryGetPropertyValue("IsLink", out isLink) && isLink);
        }

        public static bool IsHavingProperties(this ProjectItem source, Predicate<IEnumerable<Property>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            var isHavingProperties = predicate(source.Properties.AsEnumerable());
            return isHavingProperties;
        }

        public static bool IsHavingProperties(this ProjectItem source, Predicate<Properties> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            var isHavingProperties = predicate(source.Properties);
            return isHavingProperties;
        }

        public static ProjectItem GetProjectItemContainingFullPath(this ProjectItem source, bool isLink = false)
        {
            // get target app.config full path
            var projectItemFullPath = source.GetFullPath();
            var dte = (DTE)source.DTE;
            return
                dte.GetProjectItemHavingProperties(properties =>
                    properties.GetFullPath() == projectItemFullPath &&
                    properties.GetIsLink() == isLink);

        }

        //public static bool IsHavingProperties(this ProjectItem source, Predicate<FileProperties2> predicate)
        //{
        //    if (predicate == null)
        //    {
        //        throw new ArgumentNullException("predicate");
        //    }
        //    var isHavingProperties = predicate.Invoke(source.Properties.AsFileProperties());
        //    return isHavingProperties;
        //}

        //public static ProjectItem GetHavingProperties(this ProjectItem source, Predicate<FileProperties2> predicate)
        //{
        //    if (predicate == null)
        //    {
        //        throw new ArgumentNullException("predicate");
        //    }
        //    // check current leaf
        //    var havingProperties = source.IsHavingProperties(predicate);
        //    if (havingProperties) return source;
        //    // check branch
        //    return source.ProjectItems.GetProjectItemHavingProperties(predicate);
        //}
    }
}