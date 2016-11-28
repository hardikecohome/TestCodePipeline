using System;
using System.Configuration;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.DataAccess;
using DealnetPortal.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Api.Tests.Aspire
{
    [TestClass]
    public class AspireDbTest
    {
        private IAspireStorageService _aspireStorageService;
        private IDatabaseService _databaseService;
        private Mock<ILoggingService> _loggingServiceMock;

        [TestInitialize]
        public void Intialize()
        {
            _loggingServiceMock = new Mock<ILoggingService>();
            _databaseService = new MsSqlDatabaseService(ConfigurationManager.ConnectionStrings["AspireConnection"]?.ConnectionString);                
            _aspireStorageService = new AspireStorageService(_databaseService, _loggingServiceMock.Object);
        }

        [TestMethod]
        public void TestGetGenericFieldValues()
        {
            var list = _aspireStorageService.GetGenericFieldValues();
            Assert.IsNotNull(list);
            Assert.IsTrue(list.Any());
        }
    }
}
