// Guids.cs
// MUST match guids.h
using System;

namespace GolanAvraham.ConfigurationTransform
{
    static class GuidList
    {
        public const string guidConfigurationTransformPkgString = "c347e8c1-66cb-475a-8eb4-a5a2018452fc";

        public const string guidConfigurationTransformCmdSetString = "349d3566-b19d-4234-997e-1c9e81c9f517";

        public static readonly Guid guidConfigurationTransformCmdSet = new Guid(guidConfigurationTransformCmdSetString);

        // Project menu group
        public static readonly Guid ProjectMenuGroupCmdSet = new Guid("6B8B19D3-D46F-4C88-94D6-E064A475F204");
    };
}