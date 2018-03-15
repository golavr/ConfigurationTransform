using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace GolanAvraham.ConfigurationTransform.Remove
{
    internal class RemoveCommand
    {
        private VsServices _vsServices;
        private bool _isDirty;

        public static RemoveCommand Create(IServiceProvider serviceProvider, Guid menuGroup, int commandId)
        {
            return new RemoveCommand(serviceProvider, menuGroup, commandId);
        }

        public RemoveCommand(IServiceProvider serviceProvider, Guid menuGroup, int commandId)
        {
            _vsServices = VsServices.Instance;
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (serviceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService oleMenuCommandService)
            {
                var commandIdObject = new CommandID(menuGroup, commandId);
                var oleMenuCommand = new OleMenuCommand(MenuItemCallback, commandIdObject);
                oleMenuCommandService.AddCommand(oleMenuCommand);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            _isDirty = false;
            var dte2 = DTEExtensions.GetInstance();
            var selectedItem = dte2.GetSelectedItem();
            var project = selectedItem.Project;
            var projectFullName = project.FullName;
            Remove(projectFullName);
        }

        private void Remove(string fileName)
        {
            _vsServices.OutputLine($"------ Remove old transformations from project file '{fileName}'");
            try
            {
                var projectRoot = XElement.Load(fileName, LoadOptions.SetLineInfo);
                RemoveUsingTask(projectRoot);
                RemoveAfterCompileTarget(projectRoot);
                RemoveAfterBuildTarget(projectRoot);
                RemoveAfterPublishTarget(projectRoot);
                if (_isDirty)
                {
                    projectRoot.Save(fileName);
                    _vsServices.OutputLine("------ Done removing old transformations from project file");
                }
                else
                {
                    _vsServices.OutputLine("------ Project doesn't contains transformations");
                    _vsServices.ShowMessageBox(
                        "Project doesn't contains transformations");
                }
            }
            catch (Exception e)
            {
                _vsServices.OutputLine(e.Message);
                _vsServices.ShowMessageBox(
                    "Failed to remove old transformations from project file. Please remove it manually.",
                    OLEMSGBUTTON.OLEMSGBUTTON_OK, OLEMSGICON.OLEMSGICON_CRITICAL);
            }
        }

        private void RemoveUsingTask(XElement projectRoot)
        {
            var usingTask = projectRoot.ElementsAnyNs("UsingTask").FirstOrDefault(w => w.Attribute("TaskName")?.Value == "TransformXml");

            if (usingTask != null)
            {
                _vsServices.OutputLine("Remove UsingTask");
                usingTask.Remove();
                _isDirty = true;
            }
        }

        private void RemoveAfterCompileTarget(XElement projectRoot)
        {
            var afterCompile = projectRoot.ElementsAnyNs("Target")
                .FirstOrDefault(w => w.Attribute("Name")?.Value == "AfterCompile");


            if (afterCompile.NotNull(target => target.ElementsAnyNs("TransformXml").Any()))
            {
                _vsServices.OutputLine("Remove AfterCompile");
                afterCompile.Remove();
                _isDirty = true;
            }

        }

        private void RemoveAfterBuildTarget(XElement projectRoot)
        {
            var afterBuilds = projectRoot.ElementsAnyNs("Target")
                .Where(w => w.Attribute("Name")?.Value == "AfterBuild").ToList();

            for (int i = afterBuilds.Count - 1; i >= 0; i--)
            {
                var afterBuild = afterBuilds[i];
                if (afterBuild.ElementsAnyNs("TransformXml").Any())
                {
                    var source = afterBuild.ElementsAnyNs("TransformXml").FirstOrDefault()?.Attribute("Source")?.Value;
                    _vsServices.OutputLine($"Remove AfterBuild {source}");
                    afterBuild.Remove();
                    _isDirty = true;
                }
            }
        }

        private void RemoveAfterPublishTarget(XElement projectRoot)
        {
            var afterPublishComment = projectRoot.DescendantNodes().OfType<XComment>().FirstOrDefault(w => w.Value.Contains("Override After Publish to support ClickOnce AfterPublish"));

            if (afterPublishComment != null)
            {
                afterPublishComment.Remove();
                _isDirty = true;
            }

            var afterPublish = projectRoot.ElementsAnyNs("Target")
                .FirstOrDefault(w => w.Attribute("Name")?.Value == "AfterPublish");


            if (afterPublish.NotNull(target => target.ElementsAnyNs("PropertyGroup").NotNull(pg=>pg.ElementsAnyNs("DeployedConfig").Any())))
            {
                _vsServices.OutputLine("Remove AfterPublish");
                afterPublish.Remove();
                _isDirty = true;
            }
        }
    }
}
