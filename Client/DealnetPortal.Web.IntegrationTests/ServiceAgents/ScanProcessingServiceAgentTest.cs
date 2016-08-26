using System;
using System.IO;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class ScanProcessingServiceAgentTest
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
        [Ignore]
        public void TestScanDriverLicense()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client);
            var authResult = securityServiceAgent.Authenicate(DefUserName, DefUserPassword).GetAwaiter().GetResult();
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);


            IScanProcessingServiceAgent serviceAgent = new ScanProcessingServiceAgent(_client);
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client);

            var imgRaw = File.ReadAllBytes("Img//Barcode-Driver_License.CA.jpg");
            ScanningRequest scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = imgRaw
            };            

            var result = serviceAgent.ScanDriverLicense(scanningRequest).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsTrue(result.Item1.FirstName.Contains("First"));
            Assert.IsTrue(result.Item1.LastName.Contains("Last"));

            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
        }
    }
}
