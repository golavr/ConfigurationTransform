using System.Collections;
using System.Linq;
using EnvDTE;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using GolanAvraham.ConfigurationTransform.Services.Extensions;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class ProjectItemsExtensionsTest
    {
        [TestMethod]
        public void IsItemIncluded_Returns_True()
        {
            //Arrange
            const string mockname1 = "MockName1";
            const string mockvalue = "MockValue";
            const string mockname2 = "MockName2";
            const bool boolvalue = true;

            var prop1 = new Mock<Property>();
            prop1.SetupGet(s => s.Name).Returns(mockname1);
            prop1.SetupGet(s => s.Value).Returns(mockvalue);
            var prop2 = new Mock<Property>();
            prop2.SetupGet(s => s.Name).Returns(mockname2);
            prop2.SetupGet(s => s.Value).Returns(boolvalue);

            var actualProperties = new[] {prop1.Object, prop2.Object};

            var target = new Mock<ProjectItems>() {CallBase = true};
            var targetEnumerable = target.As<IEnumerable>();
            var projectItem = new Mock<ProjectItem>();

            var properties = new Mock<Properties>();
            var propertiesEnumerable = properties.As<IEnumerable>();
            propertiesEnumerable.Setup(s => s.GetEnumerator()).Returns(actualProperties.GetEnumerator());
            projectItem.SetupGet(s => s.Properties).Returns(properties.Object);

            var emptyProjectItems = new Mock<ProjectItems>();
            emptyProjectItems.SetupGet(s => s.Count).Returns(0);
            projectItem.SetupGet(s => s.ProjectItems).Returns(emptyProjectItems.Object);
            var projectItems = new[] {projectItem.Object};

            targetEnumerable.Setup(s => s.GetEnumerator()).Returns(projectItems.GetEnumerator());

            //Act
            var actual =
                target.Object.IsProjectItemPropertiesIncluded(
                    b =>
                    b.Count(
                        property =>
                        (property.Name == mockname1 && property.Value.ToString() == mockvalue) ||
                        (property.Name == mockname2 && property.Value.ToString() == boolvalue.ToString())) == 2);

            //Assert
            Assert.IsTrue(actual);
        }
    }
}
