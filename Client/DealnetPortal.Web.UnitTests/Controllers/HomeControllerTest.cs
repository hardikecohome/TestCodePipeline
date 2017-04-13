using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Utilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DealnetPortal.Web;
using DealnetPortal.Web.Common.Culture;
using DealnetPortal.Web.Controllers;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;
using DealnetPortal.Web.ServiceAgent.Managers;
using Moq;

namespace DealnetPortal.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private Mock<IContractServiceAgent> _contractServiceAgentMock;
        private Mock<IDictionaryServiceAgent> _dictionaryServiceAgentMock;
        private Mock<ICultureManager> _cultureManagerMock;
        private Mock<IHttpApiClient> _httpApiClientMock;
        private Mock<ILoggingService> _loggingServiceMock;
        private Mock<ISettingsManager> _settingsManagerMock;
        private CultureSetterManager _cultureSetterManager;

        [TestInitialize]
        public void Intialize()
        {
            _contractServiceAgentMock = new Mock<IContractServiceAgent>();
            _dictionaryServiceAgentMock = new Mock<IDictionaryServiceAgent>();
            _cultureManagerMock = new Mock<ICultureManager>();
            _httpApiClientMock = new Mock<IHttpApiClient>();
            _loggingServiceMock = new Mock<ILoggingService>();
            _settingsManagerMock = new Mock<ISettingsManager>();
            _cultureSetterManager = new CultureSetterManager(_cultureManagerMock.Object, _httpApiClientMock.Object, _dictionaryServiceAgentMock.Object, _loggingServiceMock.Object);
        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object, _cultureSetterManager, _settingsManagerMock.Object);

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object, _cultureSetterManager, _settingsManagerMock.Object);

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            //Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object, _cultureSetterManager, _settingsManagerMock.Object);

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
