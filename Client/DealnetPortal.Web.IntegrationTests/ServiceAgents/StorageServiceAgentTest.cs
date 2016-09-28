using System;
using System.Collections.Generic;
using System.IO;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Web.IntegrationTests.ServiceAgents
{
    [TestClass]
    public class StorageServiceAgentTest
    {
        private Mock<ILoggingService> _loggingService;
        private IHttpApiClient _client;
        private const string DefUserName = "user@user.com";
        private const string DefUserPassword = "123_Qwe";

        [TestInitialize]
        public void Intialize()
        {
            _loggingService = new Mock<ILoggingService>();
            string baseUrl = System.Configuration.ConfigurationManager.AppSettings["ApiUrl"];
            _client = new HttpApiClient(baseUrl);
        }

        [TestMethod]
        public void TestUploadAgreementTemplate()
        {
            IStorageServiceAgent serviceAgent = new StorageServiceAgent(_client, _loggingService.Object);

            var pdfData = File.ReadAllBytes("SeedData//EcoHome (ON) loan agreement.pdf");
            var aggreement = new AgreementTemplateDTO()
            {
                AgreementType = AgreementType.LoanApplication,
                State = "ON",
                TemplateName = "EcoHome ON rental",
                AgreementFormRaw = pdfData
            };
            var res = serviceAgent.UploadAgreementTemplate(aggreement).GetAwaiter().GetResult();
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Item1);
        }

        [TestMethod]
        public void TestUploadAgreementTemplate2()
        {
            IStorageServiceAgent serviceAgent = new StorageServiceAgent(_client, _loggingService.Object);

            var pdfData = File.ReadAllBytes("SeedData//EcoHome (ON) rental HVAC Other Equipment.pdf");
            var aggreement = new AgreementTemplateDTO()
            {
                AgreementType = AgreementType.RentalApplication,
                State = "ON",
                TemplateName = "EcoHome ON rental",
                EquipmentTypes = new List<string>() { "ECO5", "ECO1", "ECO2", "ECO44"},
                AgreementFormRaw = pdfData
            };
            var res = serviceAgent.UploadAgreementTemplate(aggreement).GetAwaiter().GetResult();
            Assert.IsNotNull(res);
            Assert.IsNotNull(res.Item1);
        }
    }
}
