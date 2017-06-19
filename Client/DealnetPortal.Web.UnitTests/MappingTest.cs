using System;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Web.Tests
{
    [TestClass]
    public class MappingTest
    {
        [TestInitialize]
        public void Intialize()
        {
            DealnetPortal.Web.App_Start.AutoMapperConfig.Configure();
        }

        [TestMethod]
        public void AssertMapperConfiguration()
        {
            try
            {
                Mapper.AssertConfigurationIsValid();
            }
            catch (Exception e)
            {
                Assert.Fail(e.ToString());
            }
        }
    }
}
