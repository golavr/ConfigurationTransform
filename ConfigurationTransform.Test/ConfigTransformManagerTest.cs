using System;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
