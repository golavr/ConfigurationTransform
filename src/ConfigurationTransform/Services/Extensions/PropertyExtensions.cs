using System;
using System.Collections;
using System.Collections.Generic;
using EnvDTE;

namespace GolanAvraham.ConfigurationTransform.Services.Extensions
{
    public static class PropertyExtensions
    {
        public static bool IsEqual(this Property property, string name, object value)
        {
            return (property.Name == name && property.Value.Equals(value));
        }
    }
}