using System;
using System.Data.Entity.Infrastructure;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DealnetPortal.Api.Tests.Services
{
    [TestClass]
    public class ContractServiceTest
    {
        private IContractService _contractService;
        private IContractRepository _contractRepository;
        private IUnitOfWork _unitOfWork;
        private ILoggingService _loggingService;
        private IAspireService _aspireService;
        private ICustomerWalletService _customerWalletService;
        private IAspireStorageReader _aspireStorageReader;
        private ISignatureService _signatureService;
        private IMailService _mailService;
        private IDealerRepository _dealerRepository;

        [TestInitialize]
        public void Intialize()
        {
            DealnetPortal.Api.App_Start.AutoMapperConfig.Configure();
            SetupMocks();
            _contractService = new ContractService(_contractRepository, _unitOfWork, _aspireService, _aspireStorageReader, _customerWalletService, _signatureService, _mailService, _loggingService, _dealerRepository);
        }

        private void SetupMocks()
        {
            Mock<IContractRepository> contractRepositoryMock = new Mock<IContractRepository>();
            Mock<IUnitOfWork> unitOfWorkMock = new Mock<IUnitOfWork>();
            Mock<ILoggingService> loggingServiceMock = new Mock<ILoggingService>();
            Mock<ISignatureService> signatureServiceMock = new Mock<ISignatureService>();
            Mock<IMailService> mailServiceMock = new Mock<IMailService>();
            Mock<IAspireService> aspireServiceMock = new Mock<IAspireService>();
            Mock<IAspireStorageReader> aspireStorageServiceMock = new Mock<IAspireStorageReader>();
            Mock<ICustomerWalletService> customerWalletServiceMock = new Mock<ICustomerWalletService>();
            Mock<IDealerRepository> dealerRepositoryMock = new Mock<IDealerRepository>();

            contractRepositoryMock.Setup(s => s.CreateContract(It.IsAny<string>())).Returns(
                new Contract()
                {
                    ContractState = ContractState.Started,
                    Id = 1,
                    CreationTime = DateTime.Now,
                    Dealer = new ApplicationUser()
                    {
                        Id = Guid.NewGuid().ToString()
                    }
                });

            unitOfWorkMock.Setup(s => s.Save()).Verifiable();
            loggingServiceMock.Setup(s => s.LogError(It.IsAny<string>())).Verifiable();
            loggingServiceMock.Setup(s => s.LogError(It.IsAny<string>(), It.IsAny<Exception>())).Verifiable();
            loggingServiceMock.Setup(s => s.LogInfo(It.IsAny<string>())).Verifiable();
            loggingServiceMock.Setup(s => s.LogWarning(It.IsAny<string>())).Verifiable();

            _contractRepository = contractRepositoryMock.Object;
            _unitOfWork = unitOfWorkMock.Object;
            _loggingService = loggingServiceMock.Object;
            _aspireService = aspireServiceMock.Object;
            _signatureService = signatureServiceMock.Object;
            _mailService = mailServiceMock.Object;
            _aspireStorageReader = aspireStorageServiceMock.Object;
            _customerWalletService = customerWalletServiceMock.Object;
            _dealerRepository = dealerRepositoryMock.Object;
        }

        [TestMethod]
        public void TestCreateContract()
        {
            var contract = _contractService.CreateContract(Guid.NewGuid().ToString());
            Assert.IsNotNull(contract);
            Assert.AreEqual(contract.ContractState, ContractState.Started);
        }
    }
}
