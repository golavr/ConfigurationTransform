using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class PropertiesExtensions
    {
        public static IEnumerable<Property> AsEnumerable(this Properties source)
        {
            return source.Cast<Property>();
        }

        public static TResult GetPropertyValue<TResult>(this Properties source, string name)
        {
            var value = source.AsEnumerable().First(property => property.Name == name).Value;
            return (TResult)value;
        }

        public static string GetFullPath(this Properties source)
        {
            var value = source.GetPropertyValue<string>("FullPath");
            return value;
        }

        public static bool GetIsLink(this Properties source)
        {
            var value = source.GetPropertyValue<bool>("IsLink");
            return value;
        }
    }
}