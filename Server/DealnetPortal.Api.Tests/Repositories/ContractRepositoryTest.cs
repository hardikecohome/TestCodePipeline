using System;
using System.Linq;
using DealnetPortal.Api.Models;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Repositories
{
    [TestClass]
    public class ContractRepositoryTest
    {
        private IDatabaseFactory _databaseFactory;
        private IUnitOfWork _unitOfWork;
        private IContractRepository _contractRepository;
        private ApplicationUser _user;

        [TestInitialize]
        public void Intialize()
        {            
            _databaseFactory = new DatabaseFactory();
            _unitOfWork = new UnitOfWork(_databaseFactory);
            _contractRepository = new ContractRepository(_databaseFactory);
            _user = _databaseFactory.Get().Users.First();
        }

        [TestMethod]
        public void TestCreateContract()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);
            Assert.AreEqual(contract.ContractState, ContractState.Started);
            var contractId = contract.Id;
            //repository should to return the same contract for the same user, if started contract didn't changed
            contract = _contractRepository.CreateContract(_user.Id);
            Assert.AreEqual(contract.Id, contractId);
            var isDeleted = _contractRepository.DeleteContract(_user.Id, contractId);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public void TestContractAddress()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);
                        
            var address = new ContractAddress()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "1"
            };
            contract.ContractAddress = address;
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();
            Assert.IsNotNull(contract.ContractAddress);
            contract.ContractAddress = null;
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();
            Assert.IsNull(contract.ContractAddress);
            //ContractData contractData = new ContractData()
            //{
            //    Id = contract.Id,
            //    ContractAddress = address
            //};
            //bool isUpdated = _contractRepository.UpdateContractData(contractData);
            //Assert.IsTrue(isUpdated);
            //_unitOfWork.Save();
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNull(contract.ContractAddress);
            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }
    }
}
