using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Enums;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DealnetPortal.Api.Tests.Repositories
{
    //[Ignore]
    [TestClass]
    public class ContractRepositoryTest
    {
        private IDatabaseFactory _databaseFactory;
        private IUnitOfWork _unitOfWork;
        private IContractRepository _contractRepository;
        private ApplicationUser _user;

        public TestContext TestContext { get; set; }
        [ClassInitialize]
        public static void SetUp(TestContext context)
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", context.TestDeploymentDir);
        }

        [TestInitialize]
        public void Intialize()
        {            
            Database.SetInitializer(
                new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());

            _databaseFactory = new DatabaseFactory();
            _unitOfWork = new UnitOfWork(_databaseFactory);
            _contractRepository = new ContractRepository(_databaseFactory);
            _user = _databaseFactory.Get().Users.FirstOrDefault();
            if (_user == null)
            {
                _user = CreateTestUser();
                _databaseFactory.Get().Users.Add(_user);
                _unitOfWork.Save();
            }            
        }

        private ApplicationUser CreateTestUser()
        {
            var user = new ApplicationUser()
            {
                Email = "user@user.ru",
                UserName = "user@user.ru",
                EmailConfirmed = false,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
            };
            return user;
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
        public void TestUpdateContract()
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
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNotNull(contract.ContractAddress);
            contract.ContractAddress = null;
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNull(contract.ContractAddress);
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNull(contract.ContractAddress);

            contract.Customers = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Fst1",
                    LastName = "Lst1",
                    DateOfBirth = DateTime.Today
                },
                new Customer()
                {
                    FirstName = "Fst2",
                    LastName = "Lst2",
                    DateOfBirth = DateTime.Today
                }
            };
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();            
            contract = _contractRepository.GetContract(contract.Id);
            Assert.AreEqual(contract.Customers.Count, 2);

            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public void TestUpdateContractData()
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
            ContractData contractData = new ContractData()
            {
                Id = contract.Id,
                ContractAddress = address
            };
            _contractRepository.UpdateContractClientData(contract.Id, address, null);
            //_contractRepository.UpdateContractData(contractData);
            _unitOfWork.Save();
            contractData.ContractAddress = null;
            var customers = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Fst1",
                    LastName = "Lst1",
                    DateOfBirth = DateTime.Today
                },
                new Customer()
                {
                    FirstName = "Fst2",
                    LastName = "Lst2",
                    DateOfBirth = DateTime.Today
                }
            };
            contractData.Customers = customers;
            //_contractRepository.UpdateContractData(contractData);
            _contractRepository.UpdateContractClientData(contract.Id, null, customers);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.Customers.Count, 2);

            var owners = contract.Customers;
            owners.Remove(owners.First());
            owners.Last().FirstName = "Name changed";
            owners.Add(new Customer()
            {
                FirstName = "Fst3",
                LastName = "Lst3",
                DateOfBirth = DateTime.Today
            });            
            contractData.Customers = owners.ToList();
            //_contractRepository.UpdateContractData(contractData);
            _contractRepository.UpdateContractClientData(contract.Id, null, owners.ToList());
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.Customers.Count, 2);
            Assert.AreEqual(contract.Customers.First().FirstName, "Name changed");

            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }
    }
}
