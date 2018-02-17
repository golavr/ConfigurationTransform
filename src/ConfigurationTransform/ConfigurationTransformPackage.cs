using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using VSLangProj;

namespace GolanAvraham.ConfigurationTransform
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.3", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidConfigurationTransformPkgString)]
    //Specifies a UI context in which a solution exists.
    [ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]
    public sealed class ConfigurationTransformPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ConfigurationTransformPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            var mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                // Transform
                var menuCommandId = new CommandID(GuidList.guidConfigurationTransformCmdSet, (int)PkgCmdIDList.cmdidAddConfigTransforms);
                var oleMenuCommand = new OleMenuCommand(MenuItemCallback, null, BeforeQueryStatus, menuCommandId);
                mcs.AddCommand(oleMenuCommand);

                // Preview
                var previewCommandId = new CommandID(GuidList.guidConfigurationTransformCmdSet, (int)PkgCmdIDList.cmdidPreviewConfigTransforms);
                var previewOleMenuCommand = new OleMenuCommand(PreviewMenuItemCallback, null, PreviewBeforeQueryStatus, previewCommandId);
                mcs.AddCommand(previewOleMenuCommand);
            }
        }

        public override string ToString()
        {
            return "ConfigurationTransformPackage";
        }

        #endregion

        private void PreviewBeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            try
            {
                var menuCommand = sender as OleMenuCommand;
                if (menuCommand == null) return;
                menuCommand.Visible = false;

                var dte2 = DTEExtensions.GetInstance();
                if (!dte2.HasOneSelectedItem()) return;
                var selectedItem = dte2.GetSelectedItem();
                // cache selected config project
                _selectedProjectItem = selectedItem.ProjectItem;
                if (!ConfigTransformManager.IsTransformConfigName(_selectedProjectItem.Name)) return;

                menuCommand.Visible = true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Exception in PreviewBeforeQueryStatus() of: {0}. Exception message: {1}", this,
                    e.Message));
                VsServices.Instance.OutputLine(e.Message);
            }
        }

        private void PreviewMenuItemCallback(object sender, EventArgs eventArgs)
        {
            try
            {
                ConfigTransformManager.PreviewTransform(_selectedProjectItem);
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Exception in PreviewMenuItemCallback() of: {0}. Exception message: {1}", this, e.Message));
                VsServices.Instance.OutputLine(e.Message);
            }
        }

        private ProjectItem _selectedProjectItem;

        // check if we need to display config transform in context menu
        private void BeforeQueryStatus(object sender, EventArgs eventArgs)
        {
            try
            {
                var menuCommand = sender as OleMenuCommand;
                if (menuCommand == null) return;
                menuCommand.Visible = false;

                var dte2 = DTEExtensions.GetInstance();
                if (!dte2.HasOneSelectedItem()) return;
                var selectedItem = dte2.GetSelectedItem();
                // cache selected config project
                _selectedProjectItem = selectedItem.ProjectItem;
                if (_selectedProjectItem == null) return;
                if (!ConfigTransformManager.IsRootConfig(_selectedProjectItem.Name)) return;

                menuCommand.Visible = true;
            }
            catch (Exception e)
            {
                Trace.WriteLine(string.Format(CultureInfo.CurrentCulture,
                    "Exception in BeforeQueryStatus() of: {0}. Exception message: {1}", this, e.Message));
                VsServices.Instance.OutputLine(e.Message);
            }
        }

        /// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var configName = _selectedProjectItem.Name;
            VsServices.Instance.OutputLine($"------ Transform started for {configName}");

            var editProjectFile = ConfigTransformManager.EditProjectFile(_selectedProjectItem);
            const string reloadMessage = @"Changes were made in project file.";
            const string noChangeMessage = @"No changes were made.";
            var displayMessage = editProjectFile ? reloadMessage : noChangeMessage;

            VsServices.Instance.OutputLine($"------ Transform ended for {configName}");
            // Show a Message Box
            VsServices.Instance.ShowMessageBox(displayMessage);
        }

    }
}
