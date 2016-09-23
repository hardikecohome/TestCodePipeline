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

        [TestMethod]
        public void TestUpdateContractClientData()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);

            var contractData = new ContractData()
            {
                Id = contract.Id
            };

            var address = new Location()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "1"
            };
            contractData.PrimaryCustomer = new Customer()
            {
                FirstName = "FstName",
                LastName = "LstName",
                DateOfBirth = DateTime.Today
            };
            contractData.Locations = new List<Location> {address};

            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.PrimaryCustomer.Locations.Count, 1);

            var address2 = new Location()
            {
                City = "London",
                PostalCode = "348042",
                Street = "Street",
                Unit = "2",
                AddressType = AddressType.MailAddress
            };

            contractData.PrimaryCustomer = null;
            contractData.Locations = new List<Location>() {address, address2};

            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.PrimaryCustomer.Locations.Count, 2);
            address2.City = "Paris";
            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.PrimaryCustomer.Locations.Count, 2);

            var customers = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Fst1",
                    LastName = "Lst1",
                    DateOfBirth = DateTime.Today
                },
                new Customer
                {
                    FirstName = "Fst2",
                    LastName = "Lst2",
                    DateOfBirth = DateTime.Today
                }
            };
            contractData.Locations = null;
            contractData.PrimaryCustomer = null;
            contractData.SecondaryCustomers = customers;
            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.SecondaryCustomers.Count, 2);

            var owners = contract.SecondaryCustomers;
            owners.Remove(owners.First());
            owners.Last().FirstName = "Name changed";
            owners.Add(new Customer()
            {
                FirstName = "Fst3",
                LastName = "Lst3",
                DateOfBirth = DateTime.Today
            });
            contractData.SecondaryCustomers = owners.ToList();
            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = _contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.SecondaryCustomers.Count, 2);
            Assert.AreEqual(contract.SecondaryCustomers.First().FirstName, "Name changed");

            

            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }

        [TestMethod]
        public void TestUpdateContractEquipmentData()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);

            var contractData = new ContractData()
            {
                Id = contract.Id
            };

            var equipmentInfo = new EquipmentInfo
            {
                Notes = "Equipment Notes",
                RequestedTerm = "Requiested Term",
                SalesRep = "Sales Rep",
                NewEquipment = new List<NewEquipment>(),
                ExistingEquipment = new List<ExistingEquipment>()
            };
            equipmentInfo.NewEquipment.Add(new NewEquipment
            {
                Cost = 50,
                Description = "Description",
                MonthlyCost = 100,
                Quantity = 10,
                TotalMonthlyPayment = 500
            });

            equipmentInfo.ExistingEquipment.Add(new ExistingEquipment
            {
                DealerIsReplacing = true,
                EstimatedAge = "50",
                IsRental = false,
                Make = "Make",
                Model = "Model",
                Notes = "Existing Equipment notes",
                RentalCompany = "Rental company",
                SerialNumber = "Serial number",
                GeneralCondition = "General condition"
            });
            contractData.Equipment = equipmentInfo;
            this._contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.IsNotNull(contract.Equipment);
            //Assert.AreEqual(contract.Equipment.ExistingEquipment.Count, 1);
            Assert.AreEqual(contract.Equipment.NewEquipment.Count, 1);

            equipmentInfo = contract.Equipment;

            equipmentInfo.ExistingEquipment = new List<ExistingEquipment>();
            equipmentInfo.NewEquipment.First().Description = "updated value";
            equipmentInfo.NewEquipment.Add(new NewEquipment
            {
                Cost = 50,
                Description = "Description 2",
                MonthlyCost = 150,
                Quantity = 10,
                TotalMonthlyPayment = 500
            });
            contractData.Equipment = equipmentInfo;
            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.IsNotNull(contract.Equipment);
            Assert.AreEqual(contract.Equipment.ExistingEquipment.Count, 0);
            Assert.AreEqual(contract.Equipment.NewEquipment.Count, 2);

            var isDeleted = _contractRepository.DeleteContract(_user.Id, contract.Id);
            _unitOfWork.Save();
            Assert.IsTrue(isDeleted);
        }
    }
}
