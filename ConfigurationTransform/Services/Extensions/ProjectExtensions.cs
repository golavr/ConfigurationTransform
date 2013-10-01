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
    }
}