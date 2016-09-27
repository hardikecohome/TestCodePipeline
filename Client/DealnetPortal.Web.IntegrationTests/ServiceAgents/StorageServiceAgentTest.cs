using System;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Utilities;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class StorageServiceAgentTest
    {
        private Mock<ILoggingService> _loggingService;
        private IHttpApiClient _client;
        private const string DefUserName = "user@user.com";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Intialize()
        {
            _loggingService = new Mock<ILoggingService>();
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestUploadAgreementTemplate()
        {
            IStorageServiceAgent serviceAgent = new StorageServiceAgent(_client, _loggingService.Object);


        }
    }
}
