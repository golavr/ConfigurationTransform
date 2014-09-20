using System.Collections;
using System.Collections.Generic;
using EnvDTE;
using Moq;

namespace ConfigurationTransform.Test
{
    public static class MockHelper
    {
        public static Mock<ProjectItems> MockProjectItems(this IEnumerable<ProjectItem> projectItems)
        {
            var projectItemsMock = new Mock<ProjectItems>();
            projectItemsMock.As<IEnumerable>().Setup(s => s.GetEnumerator()).Returns(projectItems.GetEnumerator());

            return projectItemsMock;
        }
    }
}