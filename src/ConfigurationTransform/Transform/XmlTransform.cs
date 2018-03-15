using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public partial class XmlTransform
    {
        private static readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        public string GetTargetName(string configName, AfterTargets afterTargets)
        {
            if (string.IsNullOrEmpty(configName)) throw new ArgumentNullException(nameof(configName));
            return $"{configName.Replace(".", "_")}_{afterTargets.ToString()}";
        }

        public XElement GetTarget(XElement projectRoot, string targetName)
        {
            if (projectRoot == null) throw new ArgumentNullException(nameof(projectRoot));
            if (string.IsNullOrEmpty(targetName)) throw new ArgumentNullException(nameof(targetName));

            var targets = projectRoot.ElementsAnyNs("Target")
                .Where(w => w.Attribute("Name")?.Value == targetName).ToList();

            if (targets.Count <= 1) return targets.FirstOrDefault();

            var xmlLineInfo = targets.Last() as IXmlLineInfo;

            throw new XmlSchemaValidationException($"Only one Target with {targetName} is allowed in project file",
                null, xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
        }

        public XElement GetAfterCompileTarget(XElement projectRoot)
        {
            return GetTarget(projectRoot, "AfterCompile");
        }

        public bool HasTarget(XElement projectRoot, string targetName)
        {
            return GetTarget(projectRoot, targetName)
                .NotNull();
        }

        public XElement GetAfterBuildTarget(XElement projectRoot)
        {
            return GetTarget(projectRoot, "AfterBuild");
        }

        public XElement GetAfterPublishTarget(XElement projectRoot)
        {
            return GetTarget(projectRoot, "AfterPublish");
        }

        public bool HasAfterBuildTarget(XElement projectRoot)
        {
            return GetAfterBuildTarget(projectRoot)
                .NotNull();
        }

        public bool HasAfterPublishTarget(XElement projectRoot)
        {
            return GetAfterPublishTarget(projectRoot)
                .NotNull();
        }

        public bool HasUsingTaskTransformXml(XElement projectRoot)
        {
            if (projectRoot == null) throw new ArgumentNullException(nameof(projectRoot));
            return projectRoot.ElementsAnyNs("UsingTask").Any(w => w.Attribute("TaskName")?.Value == "TransformXml");
        }

        public bool HasAfterPublishTargetDeployedConfigDefenition(XElement projectRoot)
        {
            return GetAfterPublishTarget(projectRoot)
                .NotNull(target =>
                    target.ElementsAnyNs("PropertyGroup")
                        .Any(propertyGroup => propertyGroup.ElementsAnyNs("DeployedConfig").Any()));
        }

        public bool HasAfterBuildTargetTransformXml(XElement projectRoot, string source)
        {
            return GetAfterBuildTarget(projectRoot)
                .NotNull(target => target.ElementsAnyNs("TransformXml").Any(
                    transformXml => transformXml.Attribute("Source")?.Value == source));
        }

        public bool HasAfterCompileTargetTransformXml(XElement projectRoot, string source)
        {
            return GetAfterCompileTarget(projectRoot)
                .NotNull(target => target.ElementsAnyNs("TransformXml").Any(
                    transformXml => transformXml.Attribute("Source")?.Value == source));
        }

        public XElement CreateTarget(string targetName, AfterTargets afterTargets, string condition)
        {
            return CreateElement("Target",
                new XAttribute("Name", targetName),
                new XAttribute("AfterTargets", afterTargets.ToString()),
                new XAttribute("Condition", condition));
        }

        public XElement CreateTransformXml(string source, string destination, string transform)
        {
            return CreateElement("TransformXml", 
                new XAttribute("Source", source),
                new XAttribute("Destination", destination),
                new XAttribute("Transform", transform));
        }

        public IEnumerable<object> CreateAfterPublishContent()
        {
            const string publishComment = "Publish copies the untransformed App.config to deployment directory so overwrite it";

            return new List<object>
            {
                CreateElement("PropertyGroup", CreateElement("DeployedConfig", @"$(_DeploymentApplicationDir)$(TargetName)$(TargetExt).config$(_DeploymentFileMappingExtension)")),
                new XComment(publishComment),
                CreateElement("Copy",
                    new XAttribute("Condition", "Exists('$(DeployedConfig)')"),
                    new XAttribute("SourceFiles", "$(IntermediateOutputPath)$(TargetFileName).config"),
                    new XAttribute("DestinationFiles", "$(DeployedConfig)")
                )
            };
        }

        public IEnumerable<object> CreateAfterCompileContent(string source, string destination, string transform)
        {
            const string generateComment = @"Generate transformed app config in the intermediate directory";
            const string forceComment = @"Force build process to use the transformed configuration file from now on.";

            return new List<object>
            {
                new XComment(generateComment),
                CreateTransformXml(source, destination, transform),
                new XComment(forceComment),
                CreateElement("ItemGroup",
                    CreateElement("AppConfigWithTargetPath",
                        new XAttribute("Remove", "App.config")),
                    CreateElement("AppConfigWithTargetPath",
                        new XAttribute("Include", destination),
                        CreateElement("TargetPath", "$(TargetFileName).config")))
            };
        }

        public XElement CreateUsingTaskTransformXml()
        {
            const string transformAssemblyFile =
            @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.Web.Publishing.Tasks.dll";

            return CreateElement("UsingTask", new XAttribute("TaskName", "TransformXml"),
                new XAttribute("AssemblyFile", transformAssemblyFile));
        }

        private XElement CreateElement(string name, params object[] content)
        {
            return new XElement(Namespace + name, content);
        }
    }
}