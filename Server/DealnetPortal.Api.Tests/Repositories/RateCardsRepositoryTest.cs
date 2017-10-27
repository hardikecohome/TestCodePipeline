using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Repositories
{
    [TestClass]
    public class RateCardsRepositoryTest : BaseRepositoryTest
    {
        protected IRateCardsRepository _rateCardsRepository;
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.Combine(context.TestDeploymentDir, string.Empty));
        }

        [TestInitialize]
        public void Initialize()
        {
            InitializeTestDatabase();
            _rateCardsRepository = new RateCardsRepository(_databaseFactory);
        }

        [TestMethod]
        public void TestGetTierByDealerId()
        {
            var id = "d6d2142f-5339-4ca4-84f3-17c089d171a5";
            var tier = _rateCardsRepository.GetTierByDealerId(id, DateTime.Now);
            Assert.IsNotNull(tier);
            Assert.IsTrue(tier.RateCards.Any());
        }
    }
}
