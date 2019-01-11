using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using GolanAvraham.ConfigurationTransform.Transform;
using GolanAvraham.ConfigurationTransform.Wrappers;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class ConfigTransformManagerTest
    {
        const string JustConfig = @"config";
        const string CsFile = @"mockfile.cs";
        const string RootAppConfig = @"app.config";
        const string RootAnyConfig = @"any.config";
        const string TransformAppConfig = @"app.MockBuild.config";
        const string TransformWithoutConfigExtension = @"app.MockBuild";
        const string FileWith3Dots = @"mockfile.mockMiddle.mock";
        const string FileWithMoreDots = @"file.with.more.dots.config";

        const string BuildMock = @"MockBuild";

        const string FullPath = "FullPath";
        const string IsLink = "IsLink";

        private static readonly string[] ConfigNames = new string[] {"Debug", "Release"};

        [TestMethod]
        public void GetTransformConfigName_Success()
        {
            //Arrange
            const string sourceConfigName = RootAppConfig;
            const string buildConfigurationName = BuildMock;
            const string expected = @"app.MockBuild.config";

            //Act
            var actual = ConfigTransformManager.GetTransformConfigName(sourceConfigName, buildConfigurationName);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void GetTransformConfigName_Throws_NotSupportedException()
        {
            //Arrange
            const string sourceConfigName = JustConfig;
            const string buildConfigurationName = BuildMock;

            //Act
            ConfigTransformManager.GetTransformConfigName(sourceConfigName, buildConfigurationName);
        }

        [TestMethod]
        public void GetTransformConfigName_WhenMoreDots_Success()
        {
            //Arrange
            const string sourceConfigName = FileWithMoreDots;
            const string buildConfigurationName = BuildMock;
            const string expected = @"file.with.more.dots.MockBuild.config";

            //Act
            var actual = ConfigTransformManager.GetTransformConfigName(sourceConfigName, buildConfigurationName);

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void IsRootAppConfig_WhenAppAndConfig_ReturnTrue()
        {
            //Arrange
            const string sourceConfigName = RootAppConfig;

            //Act
            var actual = ConfigTransformManager.IsRootAppConfig(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsRootAppConfig_WhenNotAppAndConfig_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = RootAnyConfig;

            //Act
            var actual = ConfigTransformManager.IsRootAppConfig(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsRootConfig_WhenAppAndConfig_ReturnTrue()
        {
            //Arrange
            const string sourceConfigName = RootAppConfig;

            //Act
            var actual = ConfigTransformManager.IsRootConfig(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsRootConfig_WhenAnyNameAndConfig_ReturnTrue()
        {
            //Arrange
            const string sourceConfigName = RootAnyConfig;

            //Act
            var actual = ConfigTransformManager.IsRootConfig(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsRootConfig_WhenHavingConfigurationInNameAndNotEndingWithConfig_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = TransformWithoutConfigExtension;

            //Act
            bool actual;
            actual = ConfigTransformManager.IsRootConfig(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsRootConfig_WhenItIsTransformConfig_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = TransformAppConfig;

            //Act
            var actual = ConfigTransformManager.IsRootConfig(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsRootConfig_WhenItIsCsFile_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = CsFile;

            //Act
            var actual = ConfigTransformManager.IsRootConfig(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsTransformConfigName_WhenHavingConfigurationInName_ReturnTrue()
        {
            //Arrange
            const string sourceConfigName = TransformAppConfig;

            //Act
            var actual = ConfigTransformManager.IsTransformConfigName(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsTransformConfigName_WhenNotHavingConfigurationInName_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = RootAppConfig;

            //Act
            var actual = ConfigTransformManager.IsTransformConfigName(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsTransformConfigName_WhenNotEndingWithConfig_ReturnFalse()
        {
            //Arrange
            const string sourceConfigName = FileWith3Dots;

            //Act
            var actual = ConfigTransformManager.IsTransformConfigName(sourceConfigName);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CreateLinkedConfigFiles_WhenAppConfig_ThenAddFilesAndSaveTargetProject()
        {
            //Arrange
            var solutionTestHelper = new SolutionTestHelper();
            solutionTestHelper.CreateSolution();

            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(wrapper => wrapper.Exists(It.IsAny<string>())).Returns(true);
            ConfigTransformManager.FileWrapper = fileWrapperMock.Object;

            //Act
            ConfigTransformManager.CreateLinkedConfigFiles(solutionTestHelper.AppConfigProjectItemMock.Object);

            //Assert
            solutionTestHelper.AppConfigProjectItemChildsMock.Verify(v => v.AddFromFile(@"c:\myproject\my.common\app.debug.config"));
            solutionTestHelper.AppConfigProjectItemChildsMock.Verify(v => v.AddFromFile(@"c:\myproject\my.common\app.release.config"));
        }

        [TestMethod]
        public void CreateLinkedConfigFiles_WhenAnyConfig_ThenAddFilesAndSaveTargetProject()
        {
            //Arrange
            var solutionTestHelper = new SolutionTestHelper();
            solutionTestHelper.CreateSolution();

            var fileWrapperMock = new Mock<IFileWrapper>();
            fileWrapperMock.Setup(wrapper => wrapper.Exists(It.IsAny<string>())).Returns(true);
            ConfigTransformManager.FileWrapper = fileWrapperMock.Object;

            //Act
            ConfigTransformManager.CreateLinkedConfigFiles(solutionTestHelper.AnyConfigProjectItemMock.Object);

            //Assert
            solutionTestHelper.AnyConfigProjectItemChildsMock.Verify(v => v.AddFromFile(@"c:\myproject\my.common\any.debug.config"));
            solutionTestHelper.AnyConfigProjectItemChildsMock.Verify(v => v.AddFromFile(@"c:\myproject\my.common\any.release.config"));
        }

        [TestMethod]
        public void EditProjectFile_WhenAppConfig_ThenShowMessageBoxAndAddXmlDataToProjectFileAndSave()
        {
            //Arrange
            var vsServices = new Mock<IVsServices>();
            var projectXml = new Mock<IVsProjectXmlTransform>();

            ConfigTransformManager.VsService = vsServices.Object;
            ConfigTransformManager.ProjectXmlTransform = projectXml.Object;

            vsServices.Setup(
                s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                 OLEMSGICON.OLEMSGICON_QUERY)).Returns(6).Verifiable();

            var solutionTestHelper = new SolutionTestHelper();
            solutionTestHelper.CreateSolution();

            //Act
            var isSaved = ConfigTransformManager.EditProjectFile(solutionTestHelper.AppConfigProjectItemMock.Object);

            //Assert
            solutionTestHelper.ProjectTargetMock.Verify(v => v.Save(It.IsAny<string>()));
            projectXml.Verify(v => v.AddTransformTask());
            projectXml.Verify(v => v.AddAfterCompileTarget(RootAppConfig, @"..\my.common", true));
            projectXml.Verify(v => v.AddAfterPublishTarget(RootAppConfig, @"..\my.common", true));
            projectXml.Verify(v => v.Save());
        }

        [TestMethod]
        public void EditProjectFile_WhenAnyConfig_ThenShowMessageBoxAndAddXmlDataToProjectFileAndSave()
        {
            //Arrange
            var vsServices = new Mock<IVsServices>();
            var projectXml = new Mock<IVsProjectXmlTransform>();

            ConfigTransformManager.VsService = vsServices.Object;
            ConfigTransformManager.ProjectXmlTransform = projectXml.Object;

            vsServices.Setup(
                s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                 OLEMSGICON.OLEMSGICON_QUERY)).Returns(6).Verifiable();

            var solutionTestHelper = new SolutionTestHelper();
            solutionTestHelper.CreateSolution();

            //Act
            var isSaved = ConfigTransformManager.EditProjectFile(solutionTestHelper.AnyConfigProjectItemMock.Object);

            //Assert
            solutionTestHelper.ProjectTargetMock.Verify(v=>v.Save(It.IsAny<string>()));
            projectXml.Verify(v => v.AddTransformTask());
            projectXml.Verify(v => v.AddAfterBuildTarget(RootAnyConfig, @"..\my.common", true));
            projectXml.Verify(v => v.Save());
        }

        [TestMethod]
        public void GetRelativePath_Returns_RelativePath()
        {
            //Arrange
            var solutionTestHelper = new SolutionTestHelper();
            solutionTestHelper.CreateSolution();

            //Act
            var relativePath = solutionTestHelper.AppConfigProjectItemMock.Object.GetRelativePath();

            //Assert
            Assert.AreEqual(relativePath, @"..\my.common\app.config");
        }

        public class SolutionTestHelper
        {
            public Mock<Project> ProjectTargetMock { get; private set; }
            public Mock<ProjectItem> AppConfigProjectItemMock { get; private set; }
            public Mock<ProjectItems> AppConfigProjectItemChildsMock { get; private set; }
            public Mock<ProjectItem> AnyConfigProjectItemMock { get; private set; }
            public Mock<ProjectItems> AnyConfigProjectItemChildsMock { get; private set; }

            public Mock<Solution> CreateSolution(bool createWithConcreteConfigs = true)
            {
                var dte = new Mock<DTE>();
                var solution = new Mock<Solution>();

                // source
                var projectSource = createWithConcreteConfigs ? CreateProjectWithConcreteConfigs(dte.Object) : null;

                // target
                CreateProjectWithLinkedConfig(dte.Object);

                //projects - source + target
                var projects = CreateEnumerableMock<Projects, Project>(projectSource?.Object, ProjectTargetMock.Object);

                solution.SetupGet(s => s.Projects).Returns(projects.Object);
                dte.SetupGet(s => s.Solution).Returns(solution.Object);
                return solution;
            }

            private Mock<Project> CreateProjectWithConcreteConfigs(DTE dte)
            {
                // 
                // source - project with actual config
                //
                var projectSource = new Mock<Project>();

                var appConfigProjectItemMock = CreateProjectItemMock(dte, projectSource.Object, RootAppConfig,
                    @"c:\myproject\my.common\app.config", false);

                AddChildProjectItem(dte, projectSource.Object, appConfigProjectItemMock, "app.debug.config",
                    @"c:\myproject\my.common\app.debug.config");
                AddChildProjectItem(dte, projectSource.Object, appConfigProjectItemMock, "app.release.config",
                    @"c:\myproject\my.common\app.release.config");

                var anyConfigProjectItemMock = CreateProjectItemMock(dte, projectSource.Object, RootAnyConfig,
                    @"c:\myproject\my.common\any.config", false);

                AddChildProjectItem(dte, projectSource.Object, anyConfigProjectItemMock, "any.debug.config",
                    @"c:\myproject\my.common\any.debug.config");
                AddChildProjectItem(dte, projectSource.Object, anyConfigProjectItemMock, "any.release.config",
                    @"c:\myproject\my.common\any.release.config");

                //projectItems - source
                var projectItemsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(
                    appConfigProjectItemMock.Object, anyConfigProjectItemMock.Object);

                // project - source
                projectSource.SetupGet(s => s.ProjectItems).Returns(projectItemsSource.Object);
                projectSource.SetupGet(s => s.FullName).Returns(@"c:\myproject\my.common\my.common.csproj");
                return projectSource;
            }

            private Mock<ProjectItem> CreateProjectItemMock(DTE dte, Project project, string name,
                string fullPath, bool isLink)
            {
                //property - source
                var fullPathPropertyMock = CreatePropertyMock(FullPath, fullPath);
                var isLinkPropertyMock = CreatePropertyMock(IsLink, isLink);

                //properties - source
                var propertiesMock = CreateEnumerableMock<Properties, Property>(fullPathPropertyMock.Object,
                    isLinkPropertyMock.Object);

                var projectItemMock = new Mock<ProjectItem>();
                projectItemMock.SetupGet(s => s.ContainingProject).Returns(project);
                projectItemMock.SetupGet(s => s.DTE).Returns(dte);
                projectItemMock.SetupGet(s => s.Name).Returns(name);
                projectItemMock.SetupGet(s => s.Properties).Returns(propertiesMock.Object);

                return projectItemMock;
            }

            private void AddChildProjectItem(DTE dte, Project project, Mock<ProjectItem> parentProjectItem,
                string name, string fullPath, bool isLink = false)
            {
                var projectItemChilds = new List<ProjectItem>();
                if (parentProjectItem.Object.ProjectItems != null)
                {
                    projectItemChilds.AddRange(parentProjectItem.Object.ProjectItems.AsEnumerable());
                }
                var childProjectItemMock = CreateProjectItemMock(dte, project, name, fullPath, isLink);
                projectItemChilds.Add(childProjectItemMock.Object);

                var projectItemsChildsMock =
                    CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemChilds.ToArray());
                parentProjectItem.SetupGet(s => s.ProjectItems).Returns(projectItemsChildsMock.Object);
            }

            private void CreateProjectWithLinkedConfig(DTE dte)
            {
                // 
                // target - project with linked config
                //
                var projectTargetMock = new Mock<Project>();

                var appConfigProjectItemMock = CreateProjectItemMock(dte, projectTargetMock.Object, RootAppConfig,
                    @"c:\myproject\my.common\app.config", true);

                var appConfigProjectItemChildsMock = CreateEnumerableMock<ProjectItems, ProjectItem>();
                appConfigProjectItemMock.SetupGet(s => s.ProjectItems)
                    .Returns(appConfigProjectItemChildsMock.Object);

                appConfigProjectItemChildsMock.Setup(s => s.AddFromFile(It.IsAny<string>()))
                    .Callback(() => projectTargetMock.SetupGet(s => s.IsDirty).Returns(true));

                var anyConfigProjectItemMock = CreateProjectItemMock(dte, projectTargetMock.Object, RootAnyConfig,
                    @"c:\myproject\my.common\any.config", true);

                var anyConfigProjectItemChildsMock = CreateEnumerableMock<ProjectItems, ProjectItem>();
                anyConfigProjectItemMock.SetupGet(s => s.ProjectItems)
                    .Returns(anyConfigProjectItemChildsMock.Object);

                anyConfigProjectItemChildsMock.Setup(s => s.AddFromFile(It.IsAny<string>()))
                    .Callback(() => projectTargetMock.SetupGet(s => s.IsDirty).Returns(true));

                //projectItems - target
                var projectItemsTarget = CreateEnumerableMock<ProjectItems, ProjectItem>(appConfigProjectItemMock.Object, anyConfigProjectItemMock.Object);

                // project - target
                var projPropOutputType = CreatePropertyMock("OutputType", 0);
                var projProperties = CreateEnumerableMock<Properties, Property>(projPropOutputType.Object);
                projectTargetMock.SetupGet(s => s.Properties).Returns(projProperties.Object);
                projectTargetMock.SetupGet(s => s.ProjectItems).Returns(projectItemsTarget.Object);
                projectTargetMock.SetupGet(s => s.FullName).Returns(@"c:\myproject\my.console\my.console.csproj");
                var configurationManager = new Mock<ConfigurationManager>();
                configurationManager.SetupGet(s => s.ConfigurationRowNames).Returns(ConfigNames);
                projectTargetMock.SetupGet(s => s.ConfigurationManager).Returns(configurationManager.Object);

                AppConfigProjectItemMock = appConfigProjectItemMock;
                AppConfigProjectItemChildsMock = appConfigProjectItemChildsMock;
                AnyConfigProjectItemMock = anyConfigProjectItemMock;
                AnyConfigProjectItemChildsMock = anyConfigProjectItemChildsMock;
                ProjectTargetMock = projectTargetMock;
            }

            private Mock<TEnumerable> CreateEnumerableMock<TEnumerable, TItem>(params TItem[] items)
                where TEnumerable : class, IEnumerable
                where TItem : class
            {
                var enumerableMock = new Mock<TEnumerable>();
                var enumerableInterface = enumerableMock.As<IEnumerable>();
                enumerableInterface.Setup(s => s.GetEnumerator())
                    .Returns(() => items.Where(w => w != null).GetEnumerator());

                return enumerableMock;
            }

            private Mock<TEnumerable> CreateEnumerableMock<TEnumerable, TItem>()
                where TEnumerable : class, IEnumerable
                where TItem : class
            {
                var enumerableMock = CreateEnumerableMock<TEnumerable, TItem>(new TItem[] {});

                return enumerableMock;
            }

            private Mock<Property> CreatePropertyMock(string name, object value)
            {
                var property = new Mock<Property>();
                property.SetupGet(s => s.Name).Returns(name);
                property.SetupGet(s => s.Value).Returns(value);

                return property;
            }
        }

    }

}
