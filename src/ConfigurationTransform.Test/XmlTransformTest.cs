using System;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;
using FluentAssertions;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class XmlTransformTest
    {
        [TestMethod]
        public void HasUsingTaskTransformXml_Null_ThrowsException()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            Action action = () => sut.HasUsingTaskTransformXml(null);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void HasUsingTaskTransformXml_NoTransformTask_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasUsingTaskTransformXml(root);

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasUsingTaskTransformXml_TransformTask_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><UsingTask TaskName=""TransformXml""/></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasUsingTaskTransformXml(root);

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void GetTargetName_NullConfigName_ThrowsException()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            Action action = () => sut.GetTargetName(null, AfterTargets.AfterBuild);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetTargetName_ValidConfigName_ReturnValue()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            var actual = sut.GetTargetName("mock.config", AfterTargets.AfterBuild);

            //Assert
            actual.Should().Be("mock_config_AfterBuild");
        }

        [TestMethod]
        public void GetTarget_NullRoot_ThrowsException()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            Action action = () => sut.GetTarget(null, string.Empty);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }


        [TestMethod]
        public void GetTarget_EmptyName_ThrowsException()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""mock_target""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            Action action = () => sut.GetTarget(root, string.Empty);

            //Assert
            action.Should().Throw<ArgumentNullException>();
        }

        [TestMethod]
        public void GetTarget_NoTarget_ReturnsNull()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""mock_target""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.GetTarget(root, "not_mock_target");

            //Assert
            actual.Should().BeNull();
        }

        [TestMethod]
        public void GetTarget_TwoSameNameTargets_ThrowsException()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""mock_target""></Target><Target Name=""mock_target""></Target></Project>", LoadOptions.SetLineInfo);
            var sut = new XmlTransform();

            //Act
            Action action = () => sut.GetTarget(root, "mock_target");

            //Assert
            action.Should().Throw<XmlSchemaValidationException>()
                .And.LineNumber.Should().BeGreaterThan(0);
        }

        [TestMethod]
        public void GetTarget_OneTarget_ReturnsValue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""mock_target""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.GetTarget(root, "mock_target");

            //Assert
            actual.Should().NotBeNull();
        }

        [TestMethod]
        public void HasTarget_NoTarget_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasTarget(root, "mock_target");

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasTarget_TargetExist_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""mock_target""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasTarget(root, "mock_target");

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasAfterPublishTarget_NoAfterPublishTarget_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterPublishTarget(root);

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasAfterPublishTarget_OneAfterPublishTarget_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterPublish""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterPublishTarget(root);

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasAfterBuildTargetTransformXml_NoTransformXml_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterBuild""></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterBuildTargetTransformXml(root, "mock.config");

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasAfterBuildTargetTransformXml_NotEqualTransformXmlSource_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterBuild""><TransformXml Source=""plainxml.config""/></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterBuildTargetTransformXml(root, "mock.config");

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasAfterBuildTargetTransformXml_EqualTransformXmlSource_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterBuild""><TransformXml Source=""mock.config""/></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterBuildTargetTransformXml(root, "mock.config");

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasAfterPublishTargetDeployedConfigDefenition_NoDeployedConfig_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterPublish""><PropertyGroup></PropertyGroup></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterPublishTargetDeployedConfigDefenition(root);

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasAfterPublishTargetDeployedConfigDefenition_WithDeployedConfig_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterPublish""><PropertyGroup><DeployedConfig/></PropertyGroup></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterPublishTargetDeployedConfigDefenition(root);

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void HasAfterCompileTargetTransformXml_NotEqualTransformXmlSource_ReturnsFalse()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterCompile""><TransformXml Source=""plainxml.config""/></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterBuildTargetTransformXml(root, "mock.config");

            //Assert
            actual.Should().BeFalse();
        }

        [TestMethod]
        public void HasAfterCompileTargetTransformXml_EqualTransformXmlSource_ReturnsTrue()
        {
            //Arrange
            var root = XElement.Parse(@"<Project><Target Name=""AfterCompile""><TransformXml Source=""mock.config""/></Target></Project>");
            var sut = new XmlTransform();

            //Act
            var actual = sut.HasAfterCompileTargetTransformXml(root, "mock.config");

            //Assert
            actual.Should().BeTrue();
        }

        [TestMethod]
        public void CreateAfterCompileContent_AllParams_ReturnsObjectList()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            var destination = "destination";
            var actual = sut.CreateAfterCompileContent("source", destination, "transform").ToList();

            //Assert
            actual.Should().HaveCount(4);
            actual.ElementAt(0).Should().BeOfType<XComment>();
            // content already covered in TransformXml tests
            actual.ElementAt(1).Should().BeOfType<XElement>().Which.Name.LocalName.Should().Be("TransformXml");
            actual.ElementAt(2).Should().BeOfType<XComment>();
            actual.ElementAt(3).Should().BeOfType<XElement>().Subject.Name.LocalName.Should().Be("ItemGroup");
            actual.ElementAt(3).Should().BeOfType<XElement>().Subject.Elements().ElementAt(0).Should()
                .HaveAttribute("Remove", "App.config")
                .And.Subject.Name.LocalName.Should().Be("AppConfigWithTargetPath");
            actual.ElementAt(3).Should().BeOfType<XElement>().Subject.Elements().ElementAt(1).Should()
                .HaveAttribute("Include", destination)
                .And.Subject.Name.LocalName.Should().Be("AppConfigWithTargetPath");

            actual.ElementAt(3).Should().BeOfType<XElement>()
                .Subject.Elements().ElementAt(1).Elements().ElementAt(0).Should()
                .HaveValue("$(TargetFileName).config")
                .And.Subject.Name.LocalName.Should().Be("TargetPath");
        }

        [TestMethod]
        public void CreateAfterPublishContent_AllParams_ReturnsObjectList()
        {
            //Arrange
            var sut = new XmlTransform();

            //Act
            var actual = sut.CreateAfterPublishContent().ToList();

            //Assert
            actual.ElementAt(0).Should().BeOfType<XElement>().Subject.Name.LocalName.Should().Be("PropertyGroup");
            actual.ElementAt(0).Should().BeOfType<XElement>().Subject.Elements().ElementAt(0)
                .Name.LocalName.Should().Be("DeployedConfig");
            actual.ElementAt(1).Should().BeOfType<XComment>();
            actual.ElementAt(2).Should().BeOfType<XElement>().Subject.Should()
                .HaveAttribute("Condition", "Exists('$(DeployedConfig)')")
                .And.HaveAttribute("SourceFiles", "$(IntermediateOutputPath)$(TargetFileName).config")
                .And.HaveAttribute("DestinationFiles", "$(DeployedConfig)")
                .And.Subject.Name.LocalName.Should().Be("Copy");
        }
    }
}