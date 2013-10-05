using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public interface IVsProjectXmlTransform
    {
        void Open(string fileName);
        bool Save();
        void AddDependentUponConfig(string buildConfigurationName, string appConfigName);
        void AddDependentUponConfig(string[] buildConfigurationNames, string appConfigName);
        void AddTransformTask();
        void AddAfterCompileTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false);
        void AddAfterPublishTarget();
    }

    public class VsProjectXmlTransform : IVsProjectXmlTransform
    {
        private string _fileName;
        private XElement _projectRoot;
        private bool _isDirty;

        private static readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";

        private const string DeployedConfig =
            @"$(_DeploymentApplicationDir)$(TargetName)$(TargetExt).config$(_DeploymentFileMappingExtension)";

        private const string TransformAssemblyFile =
            @"$(MSBuildExtensionsPath32)\Microsoft\VisualStudio\v$(VisualStudioVersion)\Web\Microsoft.Web.Publishing.Tasks.dll";

        public virtual void Open(string fileName)
        {
            _fileName = fileName;
            _isDirty = false;
            _projectRoot = LoadProjectFile();
        }

        //public VsProjectXmlTransform(string fileName)
        //{
        //    _fileName = fileName;

        //    _projectRoot = LoadProjectFile();
        //}

        protected virtual XElement LoadProjectFile()
        {
            return XElement.Load(_fileName);
        }

        private XElement CreateElement(string name, params object[] content)
        {
            return new XElement(Namespace + name, content);
        }

        public bool Save()
        {
            if (!_isDirty) return false;
            _projectRoot.Save(_fileName);
            return true;
        }

        public void AddDependentUponConfig(string buildConfigurationName, string appConfigName)
        {
            AddDependentUponConfig(new[] { buildConfigurationName }, appConfigName);
        }

        public void AddDependentUponConfig(string[] buildConfigurationNames, string appConfigName)
        {
            // get list of configs to add
            var missingDependentUponConfigs = GetMissingDependentUponConfigs(buildConfigurationNames, appConfigName);
            // nothing to add?
            if (missingDependentUponConfigs == null || !missingDependentUponConfigs.Any()) return;
            _isDirty = true;

            var pureMissing = new List<string>();
            foreach (var dependentUponConfig in missingDependentUponConfigs)
            {
                XElement includeElement;
                // try to get include node without dependent upon
                if (TryGetIncludeConfigElement(dependentUponConfig, out includeElement))
                {
                    // included but haven't got dependent upon, so add one
                    includeElement.Add(CreateElement("DependentUpon", appConfigName));
                }
                else
                {
                    // not included
                    pureMissing.Add(dependentUponConfig);
                }
            }
            if (!pureMissing.Any()) return;

            AddConfigsToExistingItemGroupOrCreateNewOne(appConfigName, pureMissing);
        }

        protected virtual void AddConfigsToExistingItemGroupOrCreateNewOne(string appConfigName, IEnumerable<string> pureMissing)
        {
            XElement mainAppConfigElement;
            // try to find main app config (e.g. App.config)
            if (TryGetIncludeConfigElement(appConfigName, out mainAppConfigElement))
            {
                mainAppConfigElement.AddAfterSelf(
                    pureMissing.Select(
                        s => CreateIncludeWithDependentElement(s, appConfigName)));
            }
            else
            {
                var itemGroupNode = CreateElement("ItemGroup",
                                                  pureMissing.Select(
                                                      s => CreateIncludeWithDependentElement(s, appConfigName)
                                                      ));
                _projectRoot.Add(itemGroupNode);
            }
        }

        private XElement CreateIncludeWithDependentElement(string buildConfigName, string appConfigName)
        {
            return CreateElement("None", new XAttribute("Include", buildConfigName),
                                 CreateElement("DependentUpon", appConfigName));
        }

        protected virtual bool TryGetIncludeConfigElement(string appConfigName, out XElement element)
        {
            element =
                _projectRoot.DescendantsAnyNs("None")
                            .FirstOrDefault(
                                none =>
                                (none.HasAttributes && none.Attributes("Include").Any(include => include.Value == appConfigName)));
            return element != null;
        }

        public void AddTransformTask()
        {
            // check if already exists
            if (HasTransformTask()) return;
            _isDirty = true;


            //var transformAssemblyFile = GetTransformAssemblyFile();
            var usingTaskNode = CreateElement("UsingTask", new XAttribute("TaskName", "TransformXml"),
                                             new XAttribute("AssemblyFile", TransformAssemblyFile));
            _projectRoot.Add(usingTaskNode);
        }

        public void AddAfterCompileTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false)
        {
            var appConfigSplit = appConfigName.Split('.');
            var configName = appConfigSplit[0];
            var configExt = appConfigSplit[1];
            // App.$(Configuration).config
            var configFormat = string.Format("{0}{1}.$(Configuration).{2}", relativePrefix, configName, configExt);
            var transformConfig = configFormat;
            if (!transformConfigIsLink)
            {
                transformConfig = string.Format("{0}.$(Configuration).{1}", configName, configExt);
            }

            var condition = string.Format("Exists('{0}')", configFormat);

            var appConfigWithPrefix = string.Format("{0}{1}", relativePrefix, appConfigName);
            // check if already exists
            if (HasAfterCompileTarget(condition)) return;
            _isDirty = true;

            var destination = string.Format(@"$(IntermediateOutputPath)$(TargetFileName).{0}", configExt);
            const string generateComment = @"Generate transformed app config in the intermediate directory";
            const string forceComment = @"Force build process to use the transformed configuration file from now on.";
            var targetNod = CreateElement("Target",
                                         new XAttribute("Name", "AfterCompile"),
                                         new XAttribute("Condition", condition),
                                         new XComment(generateComment),
                                         CreateElement("TransformXml", new XAttribute("Source", appConfigWithPrefix),
                                                      new XAttribute("Destination", destination),
                                                      new XAttribute("Transform", transformConfig)),
                                         new XComment(forceComment),
                                         CreateElement("ItemGroup",
                                                      CreateElement("AppConfigWithTargetPath",
                                                                   new XAttribute("Remove", appConfigName)),
                                                      CreateElement("AppConfigWithTargetPath",
                                                                   new XAttribute("Include", destination),
                                                                   CreateElement("TargetPath", "$(TargetFileName).config")))
                );
            _projectRoot.Add(targetNod);
        }

        public void AddAfterPublishTarget()
        {
            // check if already exists
            if (HasAfterPublishTarget()) return;
            _isDirty = true;

            var targetNode = CreateElement("Target",
                                         new XAttribute("Name", "AfterPublish"),
                                         CreateElement("PropertyGroup", CreateElement("DeployedConfig", DeployedConfig)),
                                         new XComment("Publish copies the untransformed App.config to deployment directory so overwrite it"),
                                         CreateElement("Copy",
                                             new XAttribute("Condition", "Exists('$(DeployedConfig)')"),
                                             new XAttribute("SourceFiles", "$(IntermediateOutputPath)$(TargetFileName).config"),
                                             new XAttribute("DestinationFiles", "$(DeployedConfig)")
                                             ));
            _projectRoot.Add(new XComment(@"Override After Publish to support ClickOnce AfterPublish. Target replaces the untransformed config file copied to the deployment directory with the transformed one."));
            _projectRoot.Add(targetNode);
        }

        private IEnumerable<string> GetMissingDependentUponConfigs(IEnumerable<string> buildConfigurationNames, string appConfigName, bool isLinkConfig = false)
        {
            var missingConfigs = new List<string>();
            foreach (var buildConfigurationName in buildConfigurationNames)
            {
                string dependentConfig = ConfigTransformManager.GetTransformConfigName(appConfigName, buildConfigurationName);

                var needToAddConfig = isLinkConfig ? !HasDependentUponConfig(dependentConfig) : !HasDependentUponConfigLink(dependentConfig);
                if (needToAddConfig)
                {
                    missingConfigs.Add(dependentConfig);
                }
            }
            return missingConfigs;
        }

        protected virtual bool HasDependentUponConfig(string buildConfig)
        {
            return
                _projectRoot.ElementsAnyNs("ItemGroup").Any(
                    itemGroup => itemGroup.HasElements &&
                                 itemGroup.ElementsAnyNs("None").Any(
                                     none => none.HasAttributes &&
                                             none.Attributes("Include").Any(include => include.Value == buildConfig) &&
                                             none.HasElements &&
                                             none.ElementsAnyNs("DependentUpon").Any()));
        }

        protected virtual bool HasDependentUponConfigLink(string buildConfig)
        {
            return
                _projectRoot.ElementsAnyNs("ItemGroup").Any(
                    itemGroup => itemGroup.HasElements &&
                                 itemGroup.ElementsAnyNs("None").Any(
                                     none => none.HasAttributes &&
                                             none.Attributes("Include")
                                                 .Any(include => include.Value.EndsWith(buildConfig)) &&
                                             none.HasElements &&
                                             none.ElementsAnyNs("Link").Any(link => link.Value == buildConfig) &&
                                             none.ElementsAnyNs("DependentUpon").Any()));
        }

        //protected virtual string GetRootConfigPath(string configName)
        //{
        //    var any =
        //        _projectRoot.ElementsAnyNs("ItemGroup")
        //                    .Where(
        //                        itemGroup =>
        //                        itemGroup.HasElements &&
        //                        itemGroup.ElementsAnyNs("None")
        //                                 .Any(
        //                                     none =>
        //                                     none.HasAttributes && none.Attributes("Include").Any() && none.HasElements &&
        //                                     none.ElementsAnyNs("Link").Any(link => link.Value == configName)));
        //}

        protected virtual bool HasTransformTask()
        {
            return
                _projectRoot.ElementsAnyNs("UsingTask").Any(
                    usingTask => usingTask.HasAttributes &&
                         usingTask.Attributes("TaskName").Any(taskName => taskName.Value == "TransformXml"));
        }

        protected virtual bool HasAfterCompileTarget(string conditionConfig)
        {
            return
                _projectRoot.ElementsAnyNs("Target").Any(
                    target => target.HasAttributes && target.Attributes("Name").Any(name => name.Value == "AfterCompile") &&
                         target.Attributes("Condition").Any(condition => condition.Value == conditionConfig) && target.HasElements &&
                         target.ElementsAnyNs("TransformXml").Any(
                             transformXml =>
                             transformXml.HasAttributes && transformXml.Attributes("Source").Any() &&
                             transformXml.Attributes("Destination").Any() &&
                             transformXml.Attributes("Transform").Any()));
        }

        protected virtual bool HasAfterPublishTarget()
        {
            return
                _projectRoot.ElementsAnyNs("Target").Any(
                    target => target.HasAttributes && target.Attributes("Name").Any(name => name.Value == "AfterPublish") &&
                         target.HasElements && target.ElementsAnyNs("PropertyGroup").Any(propertyGroup => propertyGroup.HasElements));
        }
    }
}