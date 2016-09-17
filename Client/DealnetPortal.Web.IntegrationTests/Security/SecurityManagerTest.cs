using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.Security
{
    [TestClass]
    public class SecurityManagerTest
    {
        private IHttpApiClient _client;
        private Mock<ILoggingService> _loggingService;
        private const string DefUserName = "user@ya.ru";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Init()
        {
            _loggingService = new Mock<ILoggingService>();

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
            ISecurityServiceAgent serviceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            IUserManagementServiceAgent userManagementService = new UserManagementServiceAgent(_client);
            Mock<ILoggingService> loggingService = new Mock<ILoggingService>();
            ISecurityManager securityManager = new SecurityManager(serviceAgent, userManagementService, loggingService.Object);

            var result = securityManager.Login(DefUserName, DefUserPassword).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 0);
            securityManager.Logout();
        }

        [TestMethod]
        public void HttpApiClientLoginFail()
        {
            ISecurityServiceAgent serviceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            IUserManagementServiceAgent userManagementService = new UserManagementServiceAgent(_client);
            Mock<ILoggingService> loggingService = new Mock<ILoggingService>();
            ISecurityManager securityManager = new SecurityManager(serviceAgent, userManagementService, loggingService.Object);
            var result = securityManager.Login("admin", "notadmin").GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual(result[0].Type, AlertType.Error);
        }
    }
}
