using System;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class UserManagementServiceAgentTest
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
        public void TestNonAuthorizedLogout()
        {            
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client);
            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
            // we was not Authorized
            Assert.IsFalse(logoutRes);
        }

        [TestMethod]
        public async Task TestAuthorizedLogout()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client);
            var user = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            securityServiceAgent.SetAuthorizationHeader(user);            

            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client);
            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
            Assert.IsTrue(logoutRes);
        }

        //worked, by excluded from tests as user have been created already
        [Ignore]
        [TestMethod]
        public void TestRegisterUser()
        {
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client);
            RegisterBindingModel newUser = new RegisterBindingModel()
            {
                Email = "new_user@ya.ru",
                Password = "1Q2w3e_",
                ConfirmPassword = "1Q2w3e_"
            };
            var result = userManagementServiceAgent.Register(newUser).GetAwaiter().GetResult();
            Assert.IsTrue(result);
        }
    }
}
