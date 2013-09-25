using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using GolanAvraham.ConfigurationTransform.Transform;
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
            const string fullPath = "FullPath";
            const string isLink = "IsLink";

            var dte = new Mock<DTE>();
            var solution = new Mock<Solution>();

            // 
            // source - project with linked config
            //
            var projectSource = new Mock<Project>();

            //property - source
            var propSourceFullPath = CreatePropertyMock(fullPath, "sourceFileLocation");
            var propSourceIsLink = CreatePropertyMock(isLink, false);

            //properties - source
            var propertiesSource = CreateEnumerableMock<Properties, Property>(propSourceFullPath.Object,
                                                                              propSourceIsLink.Object);

            //property - source child
            var propSourceChild1FullPath = CreatePropertyMock(fullPath, "sourceChild1FileLocation");
            var projectItemSourceChild1 = new Mock<ProjectItem>();
            var sourceChild1Properties = CreateEnumerableMock<Properties, Property>(propSourceChild1FullPath.Object);
            projectItemSourceChild1.SetupGet(s => s.Properties).Returns(sourceChild1Properties.Object);

            var propSourceChild2FullPath = CreatePropertyMock(fullPath, "sourceChild2FileLocation");
            var projectItemSourceChild2 = new Mock<ProjectItem>();
            var sourceChild2Properties = CreateEnumerableMock<Properties, Property>(propSourceChild2FullPath.Object);
            projectItemSourceChild2.SetupGet(s => s.Properties).Returns(sourceChild2Properties.Object);

            var projectItemRootSource = new Mock<ProjectItem>();
            projectItemRootSource.SetupGet(s => s.ContainingProject).Returns(projectSource.Object);
            projectItemRootSource.SetupGet(s => s.DTE).Returns(dte.Object);
            projectItemRootSource.SetupGet(s => s.Properties).Returns(propertiesSource.Object);

            // -> projectItems - source child
            var projectItemsChildsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemSourceChild1.Object,
                                                                                   projectItemSourceChild2.Object);

            //projectItem - source
            projectItemRootSource.SetupGet(s => s.ProjectItems).Returns(projectItemsChildsSource.Object);

            //projectItems - source
            var projectItemsSource = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemRootSource.Object);

            // project - source
            projectSource.SetupGet(s => s.ProjectItems).Returns(projectItemsSource.Object);

            // 
            // target - project with actual config
            //
            var projectTarget = new Mock<Project>();

            //property - target
            var propTargetFullPath = CreatePropertyMock(fullPath, "sourceFileLocation");
            var propTargetIsLink = CreatePropertyMock(isLink, true);

            //properties - target
            var propertiesTarget = CreateEnumerableMock<Properties, Property>(propTargetFullPath.Object, propTargetIsLink.Object);

            //projectItem - target
            var projectItemTarget = new Mock<ProjectItem>();
            projectItemTarget.SetupGet(s => s.ContainingProject).Returns(projectTarget.Object);
            projectItemTarget.SetupGet(s => s.DTE).Returns(dte.Object);
            projectItemTarget.SetupGet(s => s.Properties).Returns(propertiesTarget.Object);
            var projectItemsTargetChilds = CreateEnumerableMock<ProjectItems, ProjectItem>();
            projectItemTarget.SetupGet(s => s.ProjectItems)
                             .Returns(projectItemsTargetChilds.Object);

            projectItemsTargetChilds.Setup(s => s.AddFromFile(It.IsAny<string>()))
                                    .Callback(() => projectTarget.SetupGet(s=>s.IsDirty).Returns(true));

            //projectItems - target
            var projectItemsTarget = CreateEnumerableMock<ProjectItems, ProjectItem>(projectItemTarget.Object);

            // project - target
            projectTarget.SetupGet(s => s.ProjectItems).Returns(projectItemsTarget.Object);

            //projects - source + target
            var projects = CreateEnumerableMock<Projects, Project>(projectSource.Object, projectTarget.Object);

            solution.SetupGet(s => s.Projects).Returns(projects.Object);
            dte.SetupGet(s => s.Solution).Returns(solution.Object);

            //Act
            ConfigTransformManager.CreateLinkedAppConfigFiles(projectItemTarget.Object);

            //Assert
            projectItemsTargetChilds.Verify(v => v.AddFromFile("sourceChild1FileLocation"));
            projectItemsTargetChilds.Verify(v => v.AddFromFile("sourceChild2FileLocation"));

            projectTarget.Verify(v=>v.Save(""));
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
