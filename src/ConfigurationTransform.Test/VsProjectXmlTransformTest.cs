using GolanAvraham.ConfigurationTransform.Services;
using GolanAvraham.ConfigurationTransform.Transform;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigurationTransform.Test
{
    [TestClass]
    public class VsProjectXmlTransformTest
    {
        const string FileWithMoreDots = "file.with.more.dots.config";

        private VsProjectXmlTransform vsProjectXmlTransform;

        [TestInitialize]
        public void Setup()
        {
            var vsSerivces = new Mock<IVsServices>();
            vsProjectXmlTransform = new VsProjectXmlTransform(vsSerivces.Object);
        }

        [TestMethod]
        public void GetTargetTransformArgs_WhenMoreDotsConfigName_Success()
        {
            //Arrange
            var configName = FileWithMoreDots;
            var relativePrefix = @"..\my.common";
            var expected = new TargetTransformArgs {
                ConfigExt = "config",
                Transform = @"..\my.common\file.with.more.dots.$(Configuration).config",
                Source = @"..\my.common\file.with.more.dots.config",
                Destination = @"$(OutputPath)file.with.more.dots.config",
                Condition = @"Exists('..\my.common\file.with.more.dots.$(Configuration).config')"
            };

            //Act
            var args = vsProjectXmlTransform.GetTargetTransformArgs(configName, relativePrefix, true);

            //Assert
            Assert.IsNotNull(args);
            Assert.AreEqual(expected.Condition, args.Condition);
            Assert.AreEqual(expected.ConfigExt, args.ConfigExt);
            Assert.AreEqual(expected.Destination, args.Destination);
            Assert.AreEqual(expected.Source, args.Source);
            Assert.AreEqual(expected.Transform, args.Transform);
        }

    }
}
