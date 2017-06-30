using System;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Models;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class UserManagementServiceAgentTest
    {
        private Mock<ILoggingService> _loggingService;
        private Mock<IAuthenticationManager> _authenticationManagerMock;
        private IHttpApiClient _client;
        private const string DefUserName = "user@ya.ru";
        private const string DefUserPassword = "123_Qwe";
        private const string DefPortalId = "df460bb2-f880-42c9-aae5-9e3c76cdcd0f";

        [TestInitialize]
        public void Intialize()
        {
            _loggingService = new Mock<ILoggingService>();
            _authenticationManagerMock = new Mock<IAuthenticationManager>();
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestNonAuthorizedLogout()
        {            
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client, _authenticationManagerMock.Object);
            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
            // we was not Authorized
            Assert.IsFalse(logoutRes);
        }

        [TestMethod]
        public async Task TestAuthorizedLogout()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var result = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword, DefPortalId);
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 0);
            securityServiceAgent.SetAuthorizationHeader(result.Item1);

            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client, _authenticationManagerMock.Object);
            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
            Assert.IsTrue(logoutRes);
        }

        //worked, by excluded from tests as user have been created already
        [Ignore]
        [TestMethod]
        public void TestRegisterUser()
        {
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client, _authenticationManagerMock.Object);
            RegisterBindingModel newUser = new RegisterBindingModel()
            {
                Email = "user3@ya.ru"
            };
            var result = userManagementServiceAgent.Register(newUser).GetAwaiter().GetResult();
            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 0);
        }
    }
}
