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
    public class DocumentProcessingServiceAgentTest
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
        public void TestScanDriverLicense()
        {
            //ISecurityServiceAgent securityServiceAgent = new SecurityServiceAgent(_client);
            //var authResult = await securityServiceAgent.Authenicate(DefUserName, DefUserPassword);
            //securityServiceAgent.SetAuthorizationHeader(authResult.Item1);


            IDocumentProcessingServiceAgent serviceAgent = new DocumentProcessingServiceAgent(_client);
            //IUserManagementServiceAgent userManagementServiceAgent = new UserManagementServiceAgent(_client);

            var imgRaw = File.ReadAllBytes("Img//Barcode-Driver_License.CA.jpg");
            ScanningRequest scanningRequest = new ScanningRequest()
            {
                ImageForReadRaw = imgRaw
            };            

            var result = serviceAgent.GetDriverLicense(scanningRequest).GetAwaiter().GetResult();


            //var logoutRes = userManagementServiceAgent.Logout().GetAwaiter().GetResult();
        }
    }
}
