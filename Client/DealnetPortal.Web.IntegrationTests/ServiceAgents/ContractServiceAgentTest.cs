using System;
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
        private const string DefUserName = "user@ya.ru";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Intialize()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestCreateContract()
        {
            IContractServiceAgent contractServiceAgent = new ContractServiceAgent(_client);
            var result = contractServiceAgent.CreateContract().GetAwaiter().GetResult();
            
            Assert.IsNotNull(result);
        }
    }
}
