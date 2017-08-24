using System;
using System.IO;
using DealnetPortal.DataAccess.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Api.Tests.Repositories
{
    [TestClass]
    public class DealerOnboardingRepositoryTest : BaseRepositoryTest
    {
        protected IDealerOnboardingRepository _dealerOnboardingRepository;
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
            _dealerOnboardingRepository = new DealerOnboardingRepository(_databaseFactory);
        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
