using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class ProjectsExtensions
    {
        public static IEnumerable<Project> AsEnumerable(this Projects source)
        {
            return source.Cast<Project>();
        }
    }
}