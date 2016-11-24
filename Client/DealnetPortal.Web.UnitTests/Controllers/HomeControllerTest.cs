using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DealnetPortal.Web;
using DealnetPortal.Web.Controllers;
using DealnetPortal.Web.ServiceAgent;
using Moq;

namespace DealnetPortal.Web.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        private Mock<IContractServiceAgent> _contractServiceAgentMock;
        private Mock<IDictionaryServiceAgent> _dictionaryServiceAgentMock;

        [TestInitialize]
        public void Intialize()
        {
            _contractServiceAgentMock = new Mock<IContractServiceAgent>();
            _dictionaryServiceAgentMock = new Mock<IDictionaryServiceAgent>();
        }

        [TestMethod]
        public void Index()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object);

            // Act
            ViewResult result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void About()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object);

            // Act
            ViewResult result = controller.About() as ViewResult;

            // Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            // Arrange
            HomeController controller = new HomeController(_contractServiceAgentMock.Object, _dictionaryServiceAgentMock.Object);

            // Act
            ViewResult result = controller.Contact() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }
    }
}
