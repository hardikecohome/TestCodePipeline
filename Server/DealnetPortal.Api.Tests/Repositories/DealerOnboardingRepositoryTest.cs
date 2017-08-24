using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain.Dealer;
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
        public void AddNewDealerInfoTest()
        {
            var owners = new List<OwnerInfo>();
            owners.Add(new OwnerInfo()
            {
                FirstName = "First1",
                LastName = "Last1",
                Address = new Address()
                {
                    Street = "Street1"
                },
                PercentOwnership = 40
            });
            owners.Add(new OwnerInfo()
            {
                FirstName = "First2",
                LastName = "Last2",
                Address = new Address()
                {
                    Street = "Street2"
                },
                PercentOwnership = 20
            });
            var dealerInfo = new DealerInfo()
            {
                ParentSalesRep = _user,
                CompanyInfo = new CompanyInfo()
                {
                    FullLegalName = "FullName",

                },
                Owners = owners
            };
            var dInfo = _dealerOnboardingRepository.AddOrUpdateDealerInfo(dealerInfo);
            _unitOfWork.Save();

            Assert.IsNotNull(dInfo);
        }
    }
}
