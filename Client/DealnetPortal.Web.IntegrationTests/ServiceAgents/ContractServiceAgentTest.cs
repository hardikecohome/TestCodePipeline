using System;
using System.Linq;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class ContractServiceAgentTest
    {
        private IHttpApiClient _client;
        private Mock<ILoggingService> _loggingService;
        private const string DefUserName = "user@user.ru";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Intialize()
        {
            _loggingService = new Mock<ILoggingService>();

            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestCreateContractForNotAutorizedUser()
        {
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client);
            var result = contractServiceAgent.CreateContract().GetAwaiter().GetResult();
            
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 1);
            Assert.AreEqual(result.Item2.First().Type, AlertType.Error);
        }

        [TestMethod]
        public void TestCreateContractForAutorizedUser()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = securityServiceAgent.Authenicate(DefUserName, DefUserPassword).GetAwaiter().GetResult();            
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client);
            var result = contractServiceAgent.CreateContract().GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 1);
            Assert.AreEqual(result.Item2.First().Type, AlertType.Error);
        }
    }
}
