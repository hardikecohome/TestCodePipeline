using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Web.IntegrationTests.Security
{
    [TestClass]
    public class SecurityManagerTest
    {
        private IHttpApiClient _client;
        private const string DefUserName = "user@ya.ru";
        private const string DefUserPassword = "123_Qwe";

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
            IUserManagementServiceAgent userManagementService = new UserManagementServiceAgent(_client);
            ISecurityManager securityManager = new SecurityManager(serviceAgent, userManagementService);

            var user = securityManager.Login(DefUserName, DefUserPassword).GetAwaiter().GetResult();
            Assert.IsTrue(user);
            securityManager.Logout();
        }

        [TestMethod]
        public void HttpApiClientLoginFail()
        {
            ISecurityServiceAgent serviceAgent = new SecurityServiceAgent(_client);
            IUserManagementServiceAgent userManagementService = new UserManagementServiceAgent(_client);
            ISecurityManager securityManager = new SecurityManager(serviceAgent, userManagementService);
            var user = securityManager.Login("admin", "notadmin").GetAwaiter().GetResult();
            Assert.IsFalse(user);
        }
    }
}
