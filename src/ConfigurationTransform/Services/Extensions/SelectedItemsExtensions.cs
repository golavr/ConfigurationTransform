using System.Collections.Generic;
using System.Linq;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class SelectedItemsExtensions
    {
        public static IEnumerable<SelectedItem> AsEnumerable(this SelectedItems source)
        {
            return source.Cast<SelectedItem>();
        }
    }
}