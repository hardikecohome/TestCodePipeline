using System;
using System.IO;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Owin.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class ScanProcessingServiceAgentTest
    {
        private Mock<ILoggingService> _loggingService;
        private Mock<IAuthenticationManager> _authenticationManagerMock;
        private IHttpApiClient _client;
        private const string DefUserName = "user@user.com";
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
        [Ignore]
        public void TestScanDriverLicense()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = securityServiceAgent.Authenicate(DefUserName, DefUserPassword, DefPortalId).GetAwaiter().GetResult();
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);


            IScanProcessingServiceAgent serviceAgent = new ScanProcessingServiceAgent(_client, _authenticationManagerMock.Object);
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client, _authenticationManagerMock.Object);

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

        [TestMethod]
        [Ignore]
        public void TestScanVoidCheque()
        {
            ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client, _loggingService.Object);
            var authResult = securityServiceAgent.Authenicate(DefUserName, DefUserPassword, DefPortalId).GetAwaiter().GetResult();
            securityServiceAgent.SetAuthorizationHeader(authResult.Item1);

            IScanProcessingServiceAgent serviceAgent = new ScanProcessingServiceAgent(_client, _authenticationManagerMock.Object);
            IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client, _authenticationManagerMock.Object);

            var imgRaw = File.ReadAllBytes("Img//micr-b.jpg");
            ScanningRequest scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = imgRaw
            };

            var result = serviceAgent.ScanVoidCheque(scanningRequest).GetAwaiter().GetResult();

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsTrue(result.Item1.AccountNumber == "111-25710");
            Assert.IsTrue(result.Item1.BankNumber == "010" );
            Assert.IsTrue(result.Item1.TransitNumber == "30081");

            var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
        }
    }
}
