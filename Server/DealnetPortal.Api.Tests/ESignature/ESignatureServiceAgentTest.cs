using System;
using System.Collections.Generic;
using System.IO;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Integration.Services.ESignature;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ItemsChoiceType = DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.SsWeb.ItemsChoiceType;
using textField = DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.Transformation.textField;

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

            //long transId = 1617776;
            long transId = 1617917;
            var res = serviceAgent.CreateDocumentProfile(transId, "Sample", null).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestUploadDocument()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            //long dpSid = 1619725;
            long dpSid = 1622672;

            var pdfRaw = File.ReadAllBytes("Files/EcoHome (ON) 2.pdf");
            var res = serviceAgent.UploadDocument(dpSid, pdfRaw, "EcoHome.pdf").GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestInsertFormFields()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            //long dpSid = 1619725;
            long dpSid = 1622672;

            var textData = new List<TextData>()
            {
                new TextData()
                {
                    Items = new string[] {"CustomerName1"},
                    text = "First Name"
                },
                new TextData()
                {
                    Items = new string[] {"CustomerAddress"},
                    text = "Customer Address"
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

            var textFields = new List<textField>()
            {
                new textField
                {
                    name = "CustomerAddress",
                    Item = "1",
                    lowerLeftX = "100",
                    lowerLeftY = "72",
                    upperRightX = "250",
                    upperRightY = "104",
                    fontSize = "14",
                    Item1 = textFieldFontTypeFont.Arial,                    
                }
            };

            var res = serviceAgent.InsertFormFields(dpSid, textFields.ToArray(), textData.ToArray(), signBlocks.ToArray()).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestConfigureSortOrder()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            long dpSid = 1619725;


            var res = serviceAgent.ConfigureSortOrder(transId, new long[] {dpSid}).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestConfigureRoles()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            long dpSid = 1619725;

            var roles = new eoConfigureRolesRole[]
            {
                new eoConfigureRolesRole()
                {                    
                    order = "1",
                    name = "Signature1",
                    firstName = "First",
                    lastName = "Name",
                    eMail = "mkhar@yandex.ru",                                        
                    ItemsElementName = new ItemsChoiceType[] {ItemsChoiceType.securityCode},
                    Items = new string[] {"123"}
                }
            };

            var res = serviceAgent.ConfigureRoles(transId, roles).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestConfigureInvitation()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            long transId = 1617776;
            long dpSid = 1619725;

            var roles = new eoConfigureRolesRole[]
            {
                new eoConfigureRolesRole()
                {
                    order = "1",
                    name = "Signature1",
                    firstName = "First",
                    lastName = "Name",
                    eMail = "mkhar@yandex.ru",
                    ItemsElementName = new ItemsChoiceType[] {ItemsChoiceType.securityCode},
                    Items = new string[] {"123"}
                }
            };

            var res = serviceAgent.ConfigureInvitation(transId, "Signature1", "First", "Name", "mkhar@yandex.ru").GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }
    }
}
