using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Services.Extensions;
using GolanAvraham.ConfigurationTransform.Transform;
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
        const string TransformAppConfig = @"app.MockBuild.config";
        const string TransformWithoutConfigExtension = @"app.MockBuild";
        const string FileWith3Dots = @"mockfile.mockMiddle.mock";

        const string BuildMock = @"MockBuild";

        const string FullPath = "FullPath";
        const string IsLink = "IsLink";

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
        public void IsRootAppConfig_Returns_True()
        {
            //Arrange
            const string sourceConfigName = RootAppConfig;

            //Act
            var actual = ConfigTransformManager.IsRootAppConfig(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsTransformConfigName_Returns_True()
        {
            //Arrange
            const string sourceConfigName = TransformAppConfig;

            //Act
            var actual = ConfigTransformManager.IsTransformConfigName(sourceConfigName);

            //Assert
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsTransformConfigName_Returns_False()
        {
            //Arrange
            const string sourceConfigName1 = RootAppConfig;
            const string sourceConfigName2 = FileWith3Dots;

            //Act
            var actual = ConfigTransformManager.IsTransformConfigName(sourceConfigName1);
            actual &= ConfigTransformManager.IsTransformConfigName(sourceConfigName2);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void IsRootAppConfig_Returns_False()
        {
            //Arrange
            const string sourceConfigName1 = TransformWithoutConfigExtension;
            const string sourceConfigName2 = TransformAppConfig;
            const string sourceConfigName3 = CsFile;

            //Act
            bool actual;
            actual = ConfigTransformManager.IsRootAppConfig(sourceConfigName1);
            actual &= ConfigTransformManager.IsRootAppConfig(sourceConfigName2);
            actual &= ConfigTransformManager.IsRootAppConfig(sourceConfigName3);

            //Assert
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void CreateLinkedAppConfigFiles_Call_AddFromFile_On_TargetProjectLinkedFile_Save_TargetProject()
        {
            //Arrange
            Mock<ProjectItems> projectItemsTargetChilds;
            Mock<Project> projectTarget;
            var projectItemTarget = CreateSolution(out projectItemsTargetChilds, out projectTarget);

            //Act
            ConfigTransformManager.CreateLinkedAppConfigFiles(projectItemTarget.Object);

            //Assert
            projectItemsTargetChilds.Verify(v => v.AddFromFile(@"c:\myproject\my.common\app.debug.config"));
            projectItemsTargetChilds.Verify(v => v.AddFromFile(@"c:\myproject\my.common\app.release.config"));

            projectTarget.Verify(v=>v.Save(""));
        }

        [TestMethod]
        public void EditProjectFile_Call_ShowMessageBox_XmlAddTargets()
        {
            //Arrange
            var vsServices = new Mock<IVsServices>();
            var projectXml = new Mock<IVsProjectXmlTransform>();

            //projectXml.Setup(s => s.Save()).Returns(true).Verifiable();

            ConfigTransformManager.VsService = vsServices.Object;
            ConfigTransformManager.ProjectXmlTransform = projectXml.Object;

            vsServices.Setup(
                s =>
                s.ShowMessageBox(It.IsAny<string>(), It.IsAny<string>(), OLEMSGBUTTON.OLEMSGBUTTON_YESNO,
                                 OLEMSGICON.OLEMSGICON_QUERY)).Returns(6).Verifiable();

            Mock<ProjectItems> projectItemsTargetChilds;
            Mock<Project> projectTarget;
            var projectItem = CreateSolution(out projectItemsTargetChilds, out projectTarget);

            //Act
            var isSaved = ConfigTransformManager.EditProjectFile(projectItem.Object);

            //Assert
            //vsServices.Verify(v=>v.ShowMessageBox(It.IsAny<string>(),It.IsAny<string>(), OLEMSGBUTTON.OLEMSGBUTTON_YESNO, OLEMSGICON.OLEMSGICON_QUERY));
            projectXml.Verify(v => v.AddTransformTask());
            projectXml.Verify(v => v.AddAfterCompileTarget(RootAppConfig, @"..\my.common\", true));
            projectXml.Verify(v => v.AddAfterPublishTarget());//RootAppConfig));
            //Assert.IsTrue(isSaved);
        }

        [TestMethod]
        public void GetRelativePath_Returns_RelativePath()
        {
            //Arrange
            Mock<ProjectItems> projectItemsTargetChilds;
            Mock<Project> projectTarget;
            var projectItem = CreateSolution(out projectItemsTargetChilds, out projectTarget);

            //Act
            var relativePath = projectItem.Object.GetRelativePath();

            //Assert
            Assert.AreEqual(relativePath, @"..\my.common\app.config");
        }

        private static Mock<ProjectItem> CreateSolution(out Mock<ProjectItems> projectItemsTargetChilds, out Mock<Project> projectTarget)
        {
            var dte = new Mock<DTE>();
            var solution = new Mock<Solution>();

            // source
            var projectSource = CreateProjectWithConcreteConfigs(dte, @"c:\myproject\my.common\app.debug.config", @"c:\myproject\my.common\app.release.config");

            // target
            Mock<ProjectItem> projectItemTarget;
            projectTarget = CreateProjectWithLinkedConfig(dte, out projectItemTarget, out projectItemsTargetChilds);


            //projects - source + target
            var projects = CreateEnumerableMock<Projects, Project>(projectSource.Object, projectTarget.Object);

            solution.SetupGet(s => s.Projects).Returns(projects.Object);
            dte.SetupGet(s => s.Solution).Returns(solution.Object);
            return projectItemTarget;
        }

        private static Mock<Project> CreateProjectWithConcreteConfigs(Mock<DTE> dte, params string[] childsFullPaths)
        {
            // 
            // source - project with actual config
            //
            var projectSource = new Mock<Project>();

            //property - source
            var propSourceFullPath = CreatePropertyMock(FullPath, @"c:\myproject\my.common\app.config");
            var propSourceIsLink = CreatePropertyMock(IsLink, false);

            //properties - source
            var propertiesSource = CreateEnumerableMock<Properties, Property>(propSourceFullPath.Object,
                                                                              propSourceIsLink.Object);

            var projectItemChilds = new List<ProjectItem>();
            //property - source child
            foreach (var childFullPath in childsFullPaths)
            {
                var propSourceChildFullPath = CreatePropertyMock(FullPath, childFullPath);
                var projectItemSourceChild = new Mock<ProjectItem>();
                var sourceChildProperties = CreateEnumerableMock<Properties, Property>(propSourceChildFullPath.Object);
                projectItemSourceChild.SetupGet(s => s.Properties).Returns(sourceChildProperties.Object);

                projectItemChilds.Add(projectItemSourceChild.Object);
            }
            //var propSourceChild1FullPath = CreatePropertyMock(FullPath, "sourceChild1FileLocation");
            //var projectItemSourceChild1 = new Mock<ProjectItem>();
            //var sourceChild1Properties = CreateEnumerableMock<Properties, Property>(propSourceChild1FullPath.Object);
            //projectItemSourceChild1.SetupGet(s => s.Properties).Returns(sourceChild1Properties.Object);

            //var propSourceChild2FullPath = CreatePropertyMock(FullPath, "sourceChild2FileLocation");
            //var projectItemSourceChild2 = new Mock<ProjectItem>();
            //var sourceChild2Properties = CreateEnumerableMock<Properties, Property>(propSourceChild2FullPath.Object);
            //projectItemSourceChild2.SetupGet(s => s.Properties).Returns(sourceChild2Properties.Object);

            var projectItemRootSource = new Mock<ProjectItem>();
            projectItemRootSource.SetupGet(s => s.ContainingProject).Returns(projectSource.Object);
            projectItemRootSource.SetupGet(s => s.DTE).Returns(dte.Object);
            projectItemRootSource.SetupGet(s => s.Properties).Returns(propertiesSource.Object);

            // -> projectItems - source child
            //var projectItemsChildsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemSourceChild1.Object,
            //                                                                               projectItemSourceChild2.Object);
            var projectItemsChildsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemChilds.ToArray());


            //projectItem - source
            projectItemRootSource.SetupGet(s => s.ProjectItems).Returns(projectItemsChildsSource.Object);

            //projectItems - source
            var projectItemsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemRootSource.Object);

            // project - source
            projectSource.SetupGet(s => s.ProjectItems).Returns(projectItemsSource.Object);
            projectSource.SetupGet(s => s.FullName).Returns(@"c:\myproject\my.common\my.common.csproj");
            return projectSource;
        }

        private static Mock<Project> CreateProjectWithLinkedConfig(Mock<DTE> dte, out Mock<ProjectItem> projectItem,
                                                          out Mock<ProjectItems> projectItemChilds)
        {
            // 
            // target - project with linked config
            //
            var projectTarget = new Mock<Project>();

            //property - target
            var propTargetFullPath = CreatePropertyMock(FullPath, @"c:\myproject\my.common\app.config");
            var propTargetIsLink = CreatePropertyMock(IsLink, true);

            //properties - target
            var propertiesTarget = CreateEnumerableMock<Properties, Property>(propTargetFullPath.Object, propTargetIsLink.Object);

            //projectItem - target
            projectItem = new Mock<ProjectItem>();
            projectItem.SetupGet(s => s.ContainingProject).Returns(projectTarget.Object);
            projectItem.SetupGet(s => s.DTE).Returns(dte.Object);
            projectItem.SetupGet(s => s.Properties).Returns(propertiesTarget.Object);
            projectItem.SetupGet(s => s.Name).Returns(RootAppConfig);
            projectItemChilds = CreateEnumerableMock<ProjectItems, ProjectItem>();
            projectItem.SetupGet(s => s.ProjectItems)
                             .Returns(projectItemChilds.Object);

            projectItemChilds.Setup(s => s.AddFromFile(It.IsAny<string>()))
                                    .Callback(() => projectTarget.SetupGet(s => s.IsDirty).Returns(true));

            //projectItems - target
            var projectItemsTarget = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItem.Object);

            // project - target
            var projPropOutputType = CreatePropertyMock("OutputType", 0);
            var projProperties = CreateEnumerableMock<Properties, Property>(projPropOutputType.Object);
            projectTarget.SetupGet(s => s.Properties).Returns(projProperties.Object);
            projectTarget.SetupGet(s => s.ProjectItems).Returns(projectItemsTarget.Object);
            projectTarget.SetupGet(s => s.FullName).Returns(@"c:\myproject\my.console\my.console.csproj");
            return projectTarget;
        }

        private static Mock<TEnumerable> CreateEnumerableMock<TEnumerable, TItem>(params TItem[] items)
            where TEnumerable : class, IEnumerable
            where TItem : class
        {
            var enumerableMock = new Mock<TEnumerable>();
            var enumerableInterface = enumerableMock.As<IEnumerable>();
            enumerableInterface.Setup(s => s.GetEnumerator()).Returns(() => items.GetEnumerator());

            return enumerableMock;
        }

        private static Mock<TEnumerable> CreateEnumerableMock<TEnumerable, TItem>()
            where TEnumerable : class, IEnumerable
            where TItem : class
        {
            var enumerableMock = CreateEnumerableMock<TEnumerable, TItem>(new TItem[] {});

            return enumerableMock;
        }

        private static Mock<Property> CreatePropertyMock(string name, object value)
        {
            var property = new Mock<Property>();
            property.SetupGet(s => s.Name).Returns(name);
            property.SetupGet(s => s.Value).Returns(value);

            return property;
        }
    }

}
