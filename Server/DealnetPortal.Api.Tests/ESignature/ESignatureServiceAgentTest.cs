using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Api.Integration.Services.ESignature;
using DealnetPortal.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ItemsChoiceType = DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.SsWeb.ItemsChoiceType;
using textField = DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation.textField;
//using TypeSetObscureFontTypeFont = DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes.Transformation.TypeSetObscureFontTypeFont;

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
        //[Ignore]
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
            //long transId = 1617917;
            //long transId = 1617948;
            long transId = 1626613;

            var res = serviceAgent.CreateDocumentProfile(transId, "Sample", null).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestUploadDocument()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            //long transId = 1617776;
            //long dpSid = 1619725;
            //long dpSid = 1622672;
            //long transId = 1617948;
            //long dpSid = 1626610;
            long transId = 1626613;
            long dpSid = 1626614;

            var pdfRaw = File.ReadAllBytes("Files/EcoHome (ON) 2s.pdf");
            var res = serviceAgent.UploadDocument(dpSid, pdfRaw, "EcoHome.pdf").GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestInsertFormFields()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            //long transId = 1617776;
            //long dpSid = 1619725;
            //long dpSid = 1622672;
            //long transId = 1617948;
            //long dpSid = 1626610;
            long transId = 1626613;
            long dpSid = 1626614;

            var textData = new List<TextData>()
            {
                new TextData()
                {
                    Items = new string[] {"CustomerName1"},
                    text = "First Name"
                },
                new TextData()
                {
                    Items = new string[] {"CustomerAddress1"},
                    text = "Customer Address"
                }
            };

            var signBlocks = new List<SigBlock>()
            {
                new SigBlock()
                {
                    signerName = "Signature1",
                    name = "Signature1",

                    Item = "6",
                    lowerLeftX = "142",
                    lowerLeftY = "72",
                    upperRightX = "293",
                    upperRightY = "104",

                },
                ////new SigBlock()
                ////{
                //    signerName = "First Name",
                //    name = "Signature3",

                //    Item = "1",
                //    lowerLeftX = "80",
                //    lowerLeftY = "72",
                //    upperRightX = "150",
                //    upperRightY = "104"
                //}
            };

            var textFields = new List<textField>()
            {
                //new textField
                //{
                //    name = "CustomerAddress",
                //    Item = "1",
                //    lowerLeftX = "100",
                //    lowerLeftY = "72",
                //    upperRightX = "250",
                //    upperRightY = "104",
                //    fontSize = "14",
                //    Item1 = textFieldFontTypeFont.Arial,
                //    color  = "Black",
                //}
            };

            var res = serviceAgent.InsertFormFields(dpSid, textFields.ToArray(), textData.ToArray(), signBlocks.ToArray()).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestConfigureSortOrder()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            //long transId = 1617776;
            //long dpSid = 1619725;
            //long transId = 1617917;
            //long dpSid = 1622672;
            //long transId = 1617948;
            //long dpSid = 1626610;
            long transId = 1626613;
            long dpSid = 1626614;

            var res = serviceAgent.ConfigureSortOrder(transId, new long[] {dpSid}).GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void TestConfigureRoles()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            //long transId = 1617776;
            //long dpSid = 1619725;
            //long transId = 1617917;
            //long dpSid = 1622672;
            //long transId = 1617948;
            //long dpSid = 1626610;
            long transId = 1626613;
            long dpSid = 1626614;

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
                    Items = new string[] {"123"},
                    //signatureCaptureMethod = new eoConfigureRolesRoleSignatureCaptureMethod()
                    //{
                    //    Value = signatureCaptureMethodType.DRAW
                    //},
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

            //long transId = 1617776;
            //long dpSid = 1619725;
            //long transId = 1617917;
            //long dpSid = 1622672;
            //long transId = 1617948;
            //long dpSid = 1626610;
            long transId = 1626613;
            long dpSid = 1626614;

            var res = serviceAgent.ConfigureInvitation(transId, "Signature1", "First", "Name", "mkhar@yandex.ru").GetAwaiter().GetResult();

            serviceAgent.Logout().GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ComplexSigningTest()
        {
            IESignatureServiceAgent serviceAgent = new ESignatureServiceAgent(_client, _loggingServiceMock.Object);
            serviceAgent.Login(DefUserName, DefUserOrganisation, DefUserPassword).Wait();

            var resTr = serviceAgent.CreateTransaction("test transaction").GetAwaiter().GetResult();
            Assert.IsFalse(resTr.Item2.Any());
            var transId = resTr.Item1.sid;

            var resPr = serviceAgent.CreateDocumentProfile(transId, "Sample", null).GetAwaiter().GetResult();
            Assert.IsFalse(resTr.Item2.Any());
            var dpSid = resPr.Item1.sid;

            var pdfRaw = File.ReadAllBytes("Files/EcoHome (ON) 2.pdf");
            var resDv = serviceAgent.UploadDocument(dpSid, pdfRaw, "EcoHome.pdf").GetAwaiter().GetResult();
            Assert.IsFalse(resDv.Item2.Any());

            //var transId = 1627976;
            //var dpSid = 1627977;

            var textData = new List<TextData>()
            {
                new TextData()
                {
                    Items = new string[] {"CustomerName1"},
                    text = "First Name"
                },
                new TextData()
                {
                    Items = new string[] {"CustomerAddress1"},
                    text = "Customer Address"
                }
            };

            var signBlocks = new List<SigBlock>()
            {
                new SigBlock()
                {
                    signerName = "Customer",
                    name = "Signature1",

                    Item = "1",
                    lowerLeftX = "142",
                    lowerLeftY = "72",
                    upperRightX = "293",
                    upperRightY = "104",

                    //customProperty = new CustomProperty[]
                    //{
                    //    new CustomProperty()
                    //    {
                    //        name = "Role",
                    //        Value = "Signature1"
                    //    }
                    //}
                },
                ////new SigBlock()
                ////{
                //    signerName = "First Name",
                //    name = "Signature3",

                //    Item = "1",
                //    lowerLeftX = "80",
                //    lowerLeftY = "72",
                //    upperRightX = "150",
                //    upperRightY = "104"
                //}
            };

            var textFields = new List<textField>()
            {
                new textField
                {
                    name = "CustomerAddress2",
                    Item = "1",
                    lowerLeftX = "50",
                    lowerLeftY = "50",
                    upperRightX = "150",
                    upperRightY = "100",
                    fontSize = "14",
                    Item1 = textFieldFontTypeFont.Courier,
                    color  = "Black",
                }
            };            

            resDv = serviceAgent.InsertFormFields(dpSid, null, null, signBlocks.ToArray()).GetAwaiter().GetResult();
            Assert.IsFalse(resDv.Item2.Any());
            //resDv = serviceAgent.InsertFormFields(dpSid, textFields.ToArray(), null, null).GetAwaiter().GetResult();
            //Assert.IsFalse(resDv.Item2.Any());
            //resDv = serviceAgent.InsertFormFields(dpSid, null, textData.ToArray(), null).GetAwaiter().GetResult();
            //Assert.IsFalse(resDv.Item2.Any());

            var mergeRes = serviceAgent.MergeData(dpSid, textData.ToArray()).GetAwaiter().GetResult();
            Assert.IsFalse(mergeRes.Any());

            //return;

            var res = serviceAgent.ConfigureSortOrder(transId, new long[] { dpSid }).GetAwaiter().GetResult();
            Assert.IsFalse(res.Any());

            var roles = new eoConfigureRolesRole[]
            {
                new eoConfigureRolesRole()
                {
                    order = "1",
                    name = "Customer",
                    firstName = "First",
                    lastName = "Name",
                    eMail = "mkhar@yandex.ru",
                    ItemsElementName = new ItemsChoiceType[] {ItemsChoiceType.securityCode},
                    Items = new string[] {"123"},
                    required = true,
                    signatureCaptureMethod = new eoConfigureRolesRoleSignatureCaptureMethod()
                    {
                        Value = signatureCaptureMethodType.TYPE
                    },
                }
            };

            res = serviceAgent.ConfigureRoles(transId, roles).GetAwaiter().GetResult();
            Assert.IsFalse(res.Any());

            res = serviceAgent.ConfigureInvitation(transId, "Customer", "First", "Name", "mkhar@yandex.ru").GetAwaiter().GetResult();
            Assert.IsFalse(res.Any());



            serviceAgent.Logout().GetAwaiter().GetResult();

        }

    }
}
