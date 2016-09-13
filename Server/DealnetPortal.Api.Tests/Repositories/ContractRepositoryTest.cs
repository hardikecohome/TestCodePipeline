using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Reflection;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
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
            AppDomain.CurrentDomain.SetData("DataDirectory",
                Path.Combine(context.TestDeploymentDir, string.Empty));
        }

        [TestInitialize]
        public void Intialize()
        {
            Database.SetInitializer(
                new DropCreateDatabaseAlways<ApplicationDbContext>());            

            _databaseFactory = new DatabaseFactory();
            _unitOfWork = new UnitOfWork(_databaseFactory);
            _contractRepository = new ContractRepository(_databaseFactory);

            var context = _databaseFactory.Get();
            context.Database.Initialize(true);

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

        [Ignore]
        [TestMethod]
        public void TestUpdateContract()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);
                        
            var address = new Location()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "1"
            };
            contract.Locations.Add(address);
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNotNull(contract.Locations);
            contract.Locations = null;
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();
            contract = _contractRepository.GetContract(contract.Id);
            Assert.IsNull(contract.Locations);

            contract.ContractCustomers = new List<ContractCustomer>()
            {                
                new ContractCustomer()
                {
                    CustomerOrder = 1,
                    Customer = new Customer()
                    {
                        FirstName = "Fst1",
                        LastName = "Lst1",
                        DateOfBirth = DateTime.Today
                    }
                },
                new ContractCustomer()
                {
                    CustomerOrder = 1,
                    Customer = 
                        new Customer()
                        {
                            FirstName = "Fst2",
                            LastName = "Lst2",
                            DateOfBirth = DateTime.Today
                        }
                }
            };
            _contractRepository.UpdateContract(contract);
            _unitOfWork.Save();            
            contract = _contractRepository.GetContract(contract.Id);
            Assert.AreEqual(contract.ContractCustomers.Count, 2);

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

            var address = new Location()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "1"
            };
            _contractRepository.UpdateContractClientData(contract.Id, new List<Location>() { address}, null);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.Locations.Count, 1);

            var address2 = new Location()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "2",
                AddressType = AddressType.MailAddress
            };
            _contractRepository.UpdateContractClientData(contract.Id, new List<Location>() { address, address2 }, null);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.Locations.Count, 2);
            address2.City = "Paris";
            _contractRepository.UpdateContractClientData(contract.Id, new List<Location>() { address2 }, null);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.Locations.Count, 1);

            var customers = new List<ContractCustomer>()
            {
                new ContractCustomer
                {
                    CustomerOrder = 1,
                    Customer = new Customer()
                    {
                        FirstName = "Fst1",
                        LastName = "Lst1",
                        DateOfBirth = DateTime.Today
                    }
                },
                new ContractCustomer
                {
                    CustomerOrder = 2,
                    Customer = new Customer()
                    {
                        FirstName = "Fst2",
                        LastName = "Lst2",
                        DateOfBirth = DateTime.Today
                    }
                }
            };
            _contractRepository.UpdateContractClientData(contract.Id, null, customers);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.ContractCustomers.Count, 2);

            var owners = contract.ContractCustomers;
            owners.Remove(owners.First());
            owners.Last().Customer.FirstName = "Name changed";
            owners.Add(new ContractCustomer()
            {
                CustomerOrder = 2,
                Customer = new Customer()
                {
                    FirstName = "Fst3",
                    LastName = "Lst3",
                    DateOfBirth = DateTime.Today
                }
            });
            _contractRepository.UpdateContractClientData(contract.Id, null, owners.ToList());
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id);
            Assert.AreEqual(contract.ContractCustomers.Count, 2);
            Assert.AreEqual(contract.ContractCustomers.First().Customer.FirstName, "Name changed");

            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }
    }
}
