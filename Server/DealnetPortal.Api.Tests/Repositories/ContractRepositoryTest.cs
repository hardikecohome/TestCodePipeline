﻿using System;
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

            InitTestData();
        }

        private void InitTestData()
        {
            _user = _databaseFactory.Get().Users.FirstOrDefault();
            if (_user == null)
            {
                _user = CreateTestUser();
                _databaseFactory.Get().Users.Add(_user);
                _unitOfWork.Save();
            }
            SetDocumentTypes();
        }

        private void SetDocumentTypes()
        {
            var documentTypes = new[]
            {
                new DocumentType() {Description = "Signed contract", Prefix = "SC_"},
                new DocumentType() {Description = "Signed Installation certificate", Prefix = "SIC_"},
                new DocumentType() {Description = "Invoice", Prefix = "INV_"},
                new DocumentType() {Description = "Copy of Void Personal Cheque", Prefix = "VPC_"},
                new DocumentType() {Description = "Extended Warranty Form", Prefix = "EWF_"},
                new DocumentType() {Description = "Third party verification call", Prefix = "TPV_"},
                new DocumentType() {Description = "Other", Prefix = ""},
            };
            _databaseFactory.Get().DocumentTypes.AddRange(documentTypes);
            _unitOfWork.Save();
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
            //repository should not return the same contract for the same user, if started contract didn't changed
            contract = _contractRepository.CreateContract(_user.Id);
            Assert.AreNotEqual(contract.Id, contractId);
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
            //contractData.Locations = new List<Location> {address};

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
            //contractData.Locations = new List<Location>() {address, address2};

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
                RequestedTerm = 60,
                SalesRep = "Sales Rep",
                NewEquipment = new List<NewEquipment>(),
                ExistingEquipment = new List<ExistingEquipment>()
            };
            equipmentInfo.NewEquipment.Add(new NewEquipment
            {
                Cost = 50,
                Description = "Description",
                MonthlyCost = 100
            });

            equipmentInfo.ExistingEquipment.Add(new ExistingEquipment
            {
                //DealerIsReplacing = true,
                EstimatedAge = 50,
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
                MonthlyCost = 150
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

        [TestMethod]
        public void TestAddApplicants()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);

            var contractData = new ContractData()
            {
                Id = contract.Id
            };

            contractData.PrimaryCustomer = new Customer()
            {
                FirstName = "First name",
                LastName = "Last name",
                DateOfBirth = DateTime.Today
            };
            contractData.SecondaryCustomers = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Add 1 fst name",
                    LastName = "Add 1 lst name",
                    DateOfBirth = DateTime.Today
                },
                new Customer()
                {
                    FirstName = "Add 2 fst name",
                    LastName = "Add 2 lst name",
                    DateOfBirth = DateTime.Today
                }
            };

            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();

            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.SecondaryCustomers.Count, 2);
        }

        [TestMethod]
        public void TestUpdateCustomerData()
        {
            Customer customer = new Customer()
            {
                FirstName = "First",
                LastName = "Last",
                DateOfBirth = DateTime.Today,
                Phones = new List<Phone>()
                {
                    new Phone()
                    {
                        PhoneType = PhoneType.Home,
                        PhoneNum = "123"
                    }
                }
            };

            var dbCustomer = _contractRepository.UpdateCustomer(customer);
            _unitOfWork.Save();
            dbCustomer = _contractRepository.GetCustomer(dbCustomer.Id);

            customer = new Customer()
            {
                Id = dbCustomer.Id,
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                DateOfBirth = customer.DateOfBirth,
                Phones = new List<Phone>()
                {
                    new Phone()
                    {
                        PhoneType = PhoneType.Home,
                        PhoneNum = "456"
                    }
                }
            };

            _contractRepository.UpdateCustomer(customer);
            try
            {
                _unitOfWork.Save();
            }
            catch (Exception ex)
            {

            }
            dbCustomer = _contractRepository.GetCustomer(dbCustomer.Id);

        }

        [TestMethod]
        public void TestAddContractDocument()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);

            ContractDocument doc = new ContractDocument()
            {
                ContractId = contract.Id,
                DocumentName = "doc1",
                DocumentTypeId = 1
            };

            _contractRepository.AddDocumentToContract(contract.Id, doc, _user.Id);
            _unitOfWork.Save();

            var docs = _contractRepository.GetContractDocumentsList(contract.Id, _user.Id);
            Assert.AreEqual(docs.Count, 1);

            doc = new ContractDocument()
            {
                ContractId = contract.Id,
                DocumentName = "doc2",
                DocumentTypeId = 1
            };

            _contractRepository.AddDocumentToContract(contract.Id, doc, _user.Id);
            _unitOfWork.Save();

            docs = _contractRepository.GetContractDocumentsList(contract.Id, _user.Id);
            Assert.AreEqual(docs.Count, 1);

            doc = new ContractDocument()
            {
                ContractId = contract.Id,
                DocumentName = "doc2",
                DocumentTypeId = 2
            };

            _contractRepository.AddDocumentToContract(contract.Id, doc, _user.Id);
            _unitOfWork.Save();

            docs = _contractRepository.GetContractDocumentsList(contract.Id, _user.Id);
            Assert.AreEqual(docs.Count, 2);
        }

        [TestMethod]
        public void TestAddAndUpdateHomeOwners()
        {
            var contract = _contractRepository.CreateContract(_user.Id);
            _unitOfWork.Save();
            Assert.IsNotNull(contract);

            var contractData = new ContractData()
            {
                Id = contract.Id
            };
            contractData.PrimaryCustomer = new Customer()
            {
                FirstName = "First name",
                LastName = "Last name",
                DateOfBirth = DateTime.Today
            };
            contractData.SecondaryCustomers = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Add 1 fst name",
                    LastName = "Add 1 lst name",
                    DateOfBirth = DateTime.Today
                },
                new Customer()
                {
                    FirstName = "Add 2 fst name",
                    LastName = "Add 2 lst name",
                    DateOfBirth = DateTime.Today
                }
            };
            contractData.HomeOwners = new List<Customer>()
            {
                new Customer()
                {
                    FirstName = "Add 1 fst name",
                    LastName = "Add 1 lst name",
                    DateOfBirth = DateTime.Today
                }
            };

            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();

            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);

            Assert.IsNotNull(contract);
            Assert.IsNotNull(contract.PrimaryCustomer);
            Assert.IsNotNull(contract.SecondaryCustomers);
            Assert.AreEqual(contract.SecondaryCustomers.Count, 2);
            Assert.IsNotNull(contract.HomeOwners);
            Assert.AreEqual(contract.HomeOwners.Count, 1);

            contractData.HomeOwners = new List<Customer>()
            {
                new Customer() {Id = contract.PrimaryCustomer.Id},
                new Customer() {Id = contract.SecondaryCustomers.First().Id}
            };

            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();

            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.SecondaryCustomers.Count, 2);
            Assert.AreEqual(contract.HomeOwners.Count, 2);

            //try to remove 1st
            contractData.HomeOwners.RemoveAt(0);
            _contractRepository.UpdateContractData(contractData, _user.Id);
            _unitOfWork.Save();
            contract = this._contractRepository.GetContractAsUntracked(contract.Id, _user.Id);
            Assert.AreEqual(contract.HomeOwners.Count, 1);
        }
    }
}
