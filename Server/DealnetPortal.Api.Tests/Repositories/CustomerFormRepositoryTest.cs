using System;
using System.Collections.Generic;
using System.IO;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Repositories
{
    [TestClass]
    public class CustomerFormRepositoryTest : BaseRepositoryTest
    {
        protected ICustomerFormRepository _customerFormRepository;
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
            InitLanguages();            
            _customerFormRepository = new CustomerFormRepository(_databaseFactory);
        }

        private void InitLanguages()
        {
            _databaseFactory.Get().Languages.Add(new Language()
            {
                Id = (int)LanguageCode.English,
                Code = "en"
            });
            _databaseFactory.Get().Languages.Add(new Language()
            {
                Id = (int)LanguageCode.French,
                Code = "fr"
            });
            _unitOfWork.Save();
        }

        [TestMethod]
        public void TestEnableLanguages()
        {
            //enable en
            var enabledLangs = new List<Language> {new Language() {Id = (int) LanguageCode.English}};
            var updatedLink = _customerFormRepository.UpdateCustomerLinkLanguages(enabledLangs, _user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(updatedLink);
            Assert.AreEqual(updatedLink.EnabledLanguages.Count, 1);
            //disable all lang
            enabledLangs = new List<Language>();
            updatedLink = _customerFormRepository.UpdateCustomerLinkLanguages(enabledLangs, _user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(updatedLink);
            Assert.AreEqual(updatedLink.EnabledLanguages.Count, 0);
        }
    }
}
