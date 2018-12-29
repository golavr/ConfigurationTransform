using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using GolanAvraham.ConfigurationTransform.Services;

namespace GolanAvraham.ConfigurationTransform.Transform
{
    public interface IVsProjectXmlTransform
    {
        void Open(string fileName);
        bool Save();
        void AddTransformTask();
        void AddAfterCompileTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false);
        void AddAfterPublishTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false);
        void AddAfterBuildTarget(string anyConfigName, string relativePrefix = null, bool transformConfigIsLink = false);
    }

    public class VsProjectXmlTransform : IVsProjectXmlTransform
    {
        private readonly IVsServices _vsServices;
        private string _fileName;
        private XElement _projectRoot;
        private bool _isDirty;

        private static readonly XNamespace Namespace = "http://schemas.microsoft.com/developer/msbuild/2003";
        private readonly XmlTransform _xmlTransform;

        public VsProjectXmlTransform(IVsServices vsServices)
        {
            _vsServices = vsServices;
            _xmlTransform = new XmlTransform();
        }

        public virtual void Open(string fileName)
        {
            _fileName = fileName;
            _isDirty = false;
            _projectRoot = LoadProjectFile();
        }

        protected virtual XElement LoadProjectFile()
        {
            return XElement.Load(_fileName, LoadOptions.SetLineInfo);
        }

        public bool Save()
        {
            if (!_isDirty) return false;
            _projectRoot.Save(_fileName);
            return true;
        }

        public void AddTransformTask()
        {
            _vsServices.OutputLine("Check if need to create UsingTask");
            // check if already exists
            if (_xmlTransform.HasUsingTaskTransformXml(_projectRoot))
            {
                _vsServices.OutputLine("UsingTask already exists");
                return;
            }
            _isDirty = true;
            _vsServices.OutputLine("Creating UsingTask");
            var usingTaskTransformXml = _xmlTransform.CreateUsingTaskTransformXml();
            _projectRoot.Add(usingTaskTransformXml);
            _vsServices.OutputLine("Done creating UsingTask");
        }

        public void AddAfterCompileTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false)
        {
            var targetName = _xmlTransform.GetTargetName(appConfigName, AfterTargets.AfterCompile);
            _vsServices.OutputLine("Check if need to create AfterCompileTarget");
            // check if target exists
            if (_xmlTransform.HasTarget(_projectRoot, targetName))
            {
                _vsServices.OutputLine("AfterCompileTarget already exists");
                return;
            }
            _vsServices.OutputLine("Creating AfterCompileTarget");
            // target doesn't not exists, so create it
            _isDirty = true;
            var args = GetTargetTransformArgs(appConfigName, relativePrefix, transformConfigIsLink);
            var afterCompileTarget = _xmlTransform.CreateTarget(targetName, AfterTargets.AfterCompile, args.Condition);

            var destination = $@"$(IntermediateOutputPath)$(TargetFileName).{args.ConfigExt}";

            // create target content
            var afterCompileContent = _xmlTransform.CreateAfterCompileContent(
                args.Source,
                destination,
                args.Transform);
            afterCompileTarget.Add(afterCompileContent);

            // add target to project root
            _projectRoot.Add(afterCompileTarget);
            _vsServices.OutputLine("Done creating AfterCompileTarget");
        }

        public void AddAfterPublishTarget(string appConfigName, string relativePrefix = null, bool transformConfigIsLink = false)
        {
            var targetName = _xmlTransform.GetTargetName(appConfigName, AfterTargets.AfterPublish);
            _vsServices.OutputLine("Check if need to create AfterPublishTarget");
            // check if target exists
            if (_xmlTransform.HasTarget(_projectRoot, targetName))
            {
                _vsServices.OutputLine("AfterPublishTarget already exists");
                return;
            }
            _vsServices.OutputLine("Creating AfterPublishTarget");
            // target doesn't not exists, so create it
            _isDirty = true;
            var condition = GetTargetTransformArgs(appConfigName, relativePrefix, transformConfigIsLink).Condition;

            // create target element
            var afterPublishTarget = _xmlTransform.CreateTarget(targetName, AfterTargets.AfterPublish, condition);

            // create target content
            var afterPublishContent = _xmlTransform.CreateAfterPublishContent();
            afterPublishTarget.Add(afterPublishContent);
            // add comment to project root
            _projectRoot.Add(new XComment(@"Override After Publish to support ClickOnce AfterPublish. Target replaces the untransformed config file copied to the deployment directory with the transformed one."));
            // add target to project root
            _projectRoot.Add(afterPublishTarget);
            _vsServices.OutputLine("Done creating AfterPublishTarget");
        }

        public void AddAfterBuildTarget(string anyConfigName, string relativePrefix = null,
            bool transformConfigIsLink = false)
        {
            var targetName = _xmlTransform.GetTargetName(anyConfigName, AfterTargets.AfterBuild);
            _vsServices.OutputLine("Check if need to create AfterBuildTarget");
            // check if target exists
            if (_xmlTransform.HasTarget(_projectRoot, targetName))
            {
                _vsServices.OutputLine("AfterBuildTarget already exists");
                return;
            }
            _vsServices.OutputLine("Creating AfterPublishTarget");
            // target doesn't not exists, so create it
            _isDirty = true;

            var args = GetTargetTransformArgs(anyConfigName, relativePrefix, transformConfigIsLink);

            // create target element
            var afterBuildTarget = _xmlTransform.CreateTarget(targetName, AfterTargets.AfterBuild, args.Condition);

            // create TransformXml element
            var transformXml = _xmlTransform.CreateTransformXml(
                args.Source, 
                args.Destination,
                args.Transform);
            afterBuildTarget.Add(transformXml);

            // add target to project root
            _projectRoot.Add(afterBuildTarget);
            _vsServices.OutputLine("Done creating AfterPublishTarget");
        }

        public TargetTransformArgs GetTargetTransformArgs(string anyConfigName, string relativePrefix = null,
            bool transformConfigIsLink = false)
        {
            var spliterIndex = anyConfigName.LastIndexOf('.');
            if (spliterIndex < 0)
                throw new NotSupportedException(anyConfigName);

            var configName = anyConfigName.Substring(0, spliterIndex);
            var configExt = anyConfigName.Substring(spliterIndex + 1);

            if (relativePrefix != null)
            {
                relativePrefix += @"\";
            }
            string transform;

            if (transformConfigIsLink)
            {
                // ..\Shared\data.$(Configuration).config
                transform = $@"{relativePrefix}{configName}.$(Configuration).{configExt}";
            }
            else
            {
                // data.$(Configuration).config
                transform = $"{configName}.$(Configuration).{configExt}";
            }

            // Exists('..\Shared\data.$(Configuration).config')
            // Exists('data.$(Configuration).config')
            var condition = $"Exists('{transform}')";

            // ..\Shared\data.config
            var source = $"{relativePrefix}{anyConfigName}";

            // $(OutputPath)data.config
            var destination = $@"$(OutputPath){configName}.{configExt}";

            return new TargetTransformArgs
            {
                Condition = condition,
                ConfigExt = configExt,
                Destination = destination,
                Source = source,
                Transform = transform
            };
        }

    }
}