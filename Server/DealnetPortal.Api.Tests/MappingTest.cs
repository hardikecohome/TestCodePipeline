using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests
{
    [TestClass]
    public class MappingTest
    {
        [TestInitialize]
        public void Intialize()
        {
            DealnetPortal.Api.App_Start.AutoMapperConfig.Configure();
        }

        [TestMethod]
        public void TestMethod1()
        {

        }
    }
}
