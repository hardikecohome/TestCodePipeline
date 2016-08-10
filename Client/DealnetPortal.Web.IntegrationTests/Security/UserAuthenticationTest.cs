using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Web.IntegrationTests.Security
{
    [TestClass]
    public class UserAuthenticationTest
    {
        private IHttpApiClient _client;

        [TestInitialize]
        public void Init()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _client = null;
        }

        [TestMethod]
        public void HttpApiClientLoginSuccess()
        {
            ISecurityServiceAgent serviceAgent = new SecurityServiceAgent(_client);
            var user = serviceAgent.Authenicate("user@ya.ru", "123_Qwe").Result;

            //Assert.IsNotNull(user);
        }

        [TestMethod]
        public void HttpApiClientLoginFail()
        {
            SecurityServiceAgent agent007 = new SecurityServiceAgent(_client);
            var user = agent007.Authenicate("admin", "notadmin").Result;

            Assert.IsNull(user);
        }
    }
}
