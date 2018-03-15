using System;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public static class Extensions
    {
        public static bool NotNull<T>(this T source, Predicate<T> predicate = null)
        {
            return source != null && (predicate == null || predicate(source));
        }
    }
}