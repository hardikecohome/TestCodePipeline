using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Hosting;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Controllers;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Api.Tests.Controllers
{
    [TestClass]
    public class ContractControllerTest
    {
        private ContractController _contractController;
        private Mock<IContractService> _contractServiceMock;
        private Mock<ILoggingService> _loggingServiceMock;

        [TestInitialize]
        public void Intialize()
        {
            DealnetPortal.Api.App_Start.AutoMapperConfig.Configure();
            _contractServiceMock = new Mock<IContractService>();
            _loggingServiceMock = new Mock<ILoggingService>();

            _contractController = new ContractController(_loggingServiceMock.Object, _contractServiceMock.Object);
            _contractController.Request = new HttpRequestMessage();
            _contractController.Request.Properties.Add(HttpPropertyKeys.HttpConfigurationKey, new HttpConfiguration());
        }

        [TestMethod]
        public void TestCreateContract()
        {            
            // test succeeded result
            _contractServiceMock.Setup(s => s.CreateContract(It.IsAny<string>())).Returns(
                new ContractDTO()
                {});

            var responseRes = _contractController.CreateContract();
            Assert.IsNotNull(responseRes);
            var response = responseRes.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.IsSuccessStatusCode);
            Tuple<ContractDTO, IList<Alert>> contractResult;
            Assert.IsTrue(response.TryGetContentValue(out contractResult));
            Assert.IsNotNull(contractResult);
            Assert.IsNotNull(contractResult.Item1);
            Assert.IsNotNull(contractResult.Item2);
            Assert.AreEqual(contractResult.Item2.Count, 0);

            // test contract creation failed
            _contractServiceMock.Setup(s => s.CreateContract(It.IsAny<string>())).Returns((ContractDTO) null);
            responseRes = _contractController.CreateContract();
            response = responseRes.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.TryGetContentValue(out contractResult));
            Assert.IsNotNull(contractResult);
            Assert.IsNull(contractResult.Item1);
            Assert.IsNotNull(contractResult.Item2);
            Assert.AreEqual(contractResult.Item2.Count, 1);
            Assert.AreEqual(contractResult.Item2.First().Type, AlertType.Error);

            // test contract creation exception
            _contractServiceMock.Setup(s => s.CreateContract(It.IsAny<string>())).Throws(new NotImplementedException());
            responseRes = _contractController.CreateContract();
            response = responseRes.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
            Assert.IsNotNull(response);
            Assert.IsTrue(response.TryGetContentValue(out contractResult));
            Assert.IsNotNull(contractResult);
            Assert.IsNull(contractResult.Item1);
            Assert.IsNotNull(contractResult.Item2);
            Assert.AreEqual(contractResult.Item2.Count, 1);
            Assert.AreEqual(contractResult.Item2.First().Type, AlertType.Error);
        }

        [TestMethod]
        public void TestUpdateContractClientData()
        {            
            _contractServiceMock.Setup(
                s =>
                    s.UpdateContractData(It.IsAny<ContractDataDTO>()))
                .Returns(new List<Alert>() { new Alert() });

            ContractDataDTO contract = new ContractDataDTO()
            {
                Id = 1,
                Locations = new List<LocationDTO>(),
                SecondaryCustomers = new List<CustomerDTO>()
            };
            var responseRes = _contractController.UpdateContractData(contract);
            var response = responseRes.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
            Assert.IsTrue(response.IsSuccessStatusCode);
            IList<Alert> alerts = null;
            Assert.IsTrue(response.TryGetContentValue(out alerts));
            Assert.IsNotNull(alerts);
            Assert.AreEqual(alerts.Count, 1);
        }

        [TestMethod]
        public void TestGetCreditCheckResult()
        {
            _contractServiceMock.Setup(
                s =>
                    s.GetCreditCheckResult(It.IsAny<int>())
                    ).Returns(new Tuple<CreditCheckDTO, IList<Alert>>(new CreditCheckDTO(), new List<Alert>() {new Alert()}));
            var responseRes = _contractController.GetCreditCheckResult(1);
            var response = responseRes.ExecuteAsync(new CancellationToken()).GetAwaiter().GetResult();
            Assert.IsTrue(response.IsSuccessStatusCode);
            Tuple<CreditCheckDTO, IList<Alert>> result;
            Assert.IsTrue(response.TryGetContentValue(out result));
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Item1);
            Assert.IsNotNull(result.Item2);
            Assert.AreEqual(result.Item2.Count, 1);
        }
    }
}
