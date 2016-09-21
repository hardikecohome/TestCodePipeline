using System;
using System.Collections.Generic;
using System.IO;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Integration.Services.ESignature;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes;
using DealnetPortal.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Api.Tests.ESignature
{
    [TestClass]
    public class ESignatureServiceAgentTest
    {
        private IHttpApiClient _client;
        private const string DefUserName = "mkharlamov";
        private const string DefUserPassword = "mkharlamov";
        private const string DefUserOrganisation = "DealNet";

        private Mock<ILoggingService> _loggingServiceMock;

        [TestInitialize]
        public void Intialize()
        {
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["eCoreApiUrl"];
            _client = new HttpApiClient(baseUrl);

            _loggingServiceMock = new Mock<ILoggingService>();
        }

        [TestMethod]
        public void TestLogin()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        [Ignore]
        public void TestCreateTransaction()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            var res = serviceAgent.CreateTransaction("test transaction").GetAwaiter().GetResult();
            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestCreateDocumentProfile()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            var res = serviceAgent.CreateDocumentProfile(transId, "Sample", null).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestUploadDocument()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            long dpSid = 1619725;

            var pdfRaw = File.ReadAllBytes("Files/EcoHome (ON) 2.pdf");
            var res = serviceAgent.UploadDocument(dpSid, pdfRaw, "EcoHome.pdf");

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestInsertFormFields()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            long dpSid = 1619725;

            var textData = new List<TextData>()
            {
                new TextData()
                {
                    Items = new string[] {"CustomerName1"},
                    text = "First Name"
                }
            };

            var signBlocks = new List<SigBlock>()
            {
                new SigBlock()
                {
                    signerName = "First Name",
                    name = "Signature1",
                    Item = "6",
                    lowerLeftX = "142",
                    lowerLeftY = "72",
                    upperRightX = "293",
                    upperRightY = "104"
                }
            };

            serviceAgent.InsertFormFields(dpSid, textData.ToArray(), signBlocks.ToArray());

            serviceAgent.Logout().GetAwaiter().GetResult();
        }
    }
}
