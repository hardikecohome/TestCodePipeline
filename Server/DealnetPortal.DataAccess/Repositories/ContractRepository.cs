using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Policy;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Repositories
{
    public class ContractRepository : BaseRepository, IContractRepository
    {
        public ContractRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public Contract CreateContract(string contractOwnerId)
        {
            //check is new contract already created
            var contract = _dbContext.Contracts.FirstOrDefault(
                c => c.Dealer.Id == contractOwnerId && c.ContractState == ContractState.Started);
            if (contract == null)
            {
                var dealer = GetUserById(contractOwnerId);
                if (dealer != null)
                {
                    contract = new Contract()
                    {
                        ContractState = ContractState.Started,
                        CreationTime = DateTime.Now,
                        Dealer = dealer
                    };
                    _dbContext.Contracts.Add(contract);
                }
            }
            return contract;
        }

        public IList<Contract> GetContracts(string ownerUserId)
        {
            var contracts = _dbContext.Contracts
                    .Include(c => c.PrimaryCustomer)
                    .Where(c => c.Dealer.Id == ownerUserId).ToList();
            return contracts;
        }

        public Contract UpdateContractState(int contractId, string contractOwnerId, ContractState newState)
        {
            var contract = GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                contract.ContractState = newState;
                contract.LastUpdateTime = DateTime.Now;
            }
            return contract;
        }

        public Contract GetContract(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .FirstOrDefault(c => c.Id == contractId && c.Dealer.Id == contractOwnerId);
        }

        public Contract GetContractAsUntracked(int contractId, string contractOwnerId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.SecondaryCustomers)
                .Include(c => c.Equipment)
                .Include(c => c.Equipment.ExistingEquipment)
                .Include(c => c.Equipment.NewEquipment)
                .AsNoTracking().
                FirstOrDefault(c => c.Id == contractId && c.Dealer.Id == contractOwnerId);
        }

        public bool DeleteContract(string contractOwnerId, int contractId)
        {
            bool deleted = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => c.Dealer.Id == contractOwnerId && c.Id == contractId);
            if (contract != null)
            {
                //remove clients for contract?
                contract.SecondaryCustomers.Clear();

                if (contract.Equipment != null)
                {
                    var entriesForDelete = contract.Equipment.NewEquipment.ToList();
                    entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

                    var entriesForDeleteE = contract.Equipment.ExistingEquipment.ToList();
                    entriesForDeleteE.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);                    
                }                

                _dbContext.Contracts.Remove(contract);
                deleted = true;
            }
            return deleted;
        }

        public bool CleanContract(string contractOwnerId, int contractId)
        {
            bool cleaned = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => c.Dealer.Id == contractOwnerId && c.Id == contractId);
            if (contract != null)
            {
                contract.SecondaryCustomers.Clear();
                cleaned = true;
            }
            return cleaned;
        }

        public Contract UpdateContract(Contract contract, string contractOwnerId)
        {
            contract.ContractState = ContractState.CustomerInfoInputted;
            _dbContext.Entry(contract).State = EntityState.Modified;
            contract.LastUpdateTime = DateTime.Now;
            return contract;
        }

        public Contract UpdateContractData(ContractData contractData, string contractOwnerId)
        {
            if (contractData != null)
            {
                var contract = GetContract(contractData.Id, contractOwnerId);
                if (contract != null)
                {
                    if (contractData.PrimaryCustomer != null)
                    {
                        var homeOwner = AddOrUpdateCustomer(contractData.PrimaryCustomer);
                        if (homeOwner != null)
                        {
                            contract.PrimaryCustomer = homeOwner;
                            contract.ContractState = ContractState.CustomerInfoInputted;
                            contract.LastUpdateTime = DateTime.Now;
                        }
                    }

                    if (contractData.SecondaryCustomers != null)
                    {
                        AddOrUpdateAdditionalApplicants(contract, contractData.SecondaryCustomers);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.Locations != null && contract.PrimaryCustomer != null)
                    {
                        AddOrUpdateCustomerLocations(contract.PrimaryCustomer, contractData.Locations);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.Equipment != null)
                    {
                        AddOrUpdateEquipment(contract, contractData.Equipment);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.ContactInfo != null)
                    {
                        AddOrUpdateContactInfo(contract, contractData.ContactInfo);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    if (contractData.PaymentInfo != null)
                    {
                        AddOrUpdatePaymentInfo(contract, contractData.PaymentInfo);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }                    

                    return contract;
                }
            }
            return null;
        }

        public IList<EquipmentType> GetEquipmentTypes()
        {
            return _dbContext.EquipmentTypes.ToList();
        }

        private EquipmentInfo AddOrUpdateEquipment(Contract contract, EquipmentInfo equipmentInfo)
        {
            var newEquipments = equipmentInfo.NewEquipment;
            var existingEquipments = equipmentInfo.ExistingEquipment;
            var dbEquipment = _dbContext.EquipmentInfo.Find(equipmentInfo.Id);
            if (dbEquipment == null || dbEquipment.Id == 0)
            {
                equipmentInfo.ExistingEquipment = new List<ExistingEquipment>();
                equipmentInfo.NewEquipment = new List<NewEquipment>();
                equipmentInfo.Contract = contract;
                dbEquipment = _dbContext.EquipmentInfo.Add(equipmentInfo);
            }
            else
            {
                equipmentInfo.Contract = contract;
                equipmentInfo.ExistingEquipment = dbEquipment.ExistingEquipment;
                equipmentInfo.NewEquipment = dbEquipment.NewEquipment;
                _dbContext.EquipmentInfo.AddOrUpdate(equipmentInfo);
                dbEquipment = equipmentInfo;
            }          

            if (newEquipments != null)
            {
                var existingEntities =
                    dbEquipment.NewEquipment.Where(
                    a => newEquipments.Any(ne => ne.Id == a.Id)).ToList();
                var entriesForDelete = dbEquipment.NewEquipment.Except(existingEntities).ToList();
                entriesForDelete.ForEach(e => _dbContext.NewEquipment.Remove(e));

                newEquipments.ForEach(ne =>
                {
                    var curEquipment =
                        dbEquipment.NewEquipment.FirstOrDefault(eq => eq.Id == ne.Id);
                    if (curEquipment == null || ne.Id == 0)
                    {
                        dbEquipment.NewEquipment.Add(ne);
                    }
                    else
                    {
                        _dbContext.NewEquipment.AddOrUpdate(ne);
                    }
                });
            }

            if (existingEquipments != null)
            {
                var existingEntities =
                    dbEquipment.ExistingEquipment.Where(
                    a => existingEquipments.Any(ee => ee.Id == a.Id)).ToList();
                var entriesForDelete = dbEquipment.ExistingEquipment.Except(existingEntities).ToList();
                entriesForDelete.ForEach(e => _dbContext.ExistingEquipment.Remove(e));

                existingEquipments.ForEach(ee =>
                {                    
                    var curEquipment =
                        dbEquipment.ExistingEquipment.FirstOrDefault(ex => ex.Id == ee.Id);
                    if (curEquipment == null || ee.Id == 0)
                    {
                        dbEquipment.ExistingEquipment.Add(ee);
                    }
                    else
                    {
                        _dbContext.ExistingEquipment.AddOrUpdate(ee);
                    }
                });
            }            
            
            return dbEquipment;
        }        

        public ContractData GetContractData(int contractId, string contractOwnerId)
        {
            ContractData contractData = new ContractData()
            {
                Id = contractId
            };
            var contract = GetContractAsUntracked(contractId, contractOwnerId);
            if (contract != null)
            {
                contractData.Locations = contract.PrimaryCustomer?.Locations?.ToList();
                contractData.SecondaryCustomers = contract.SecondaryCustomers?.ToList();
                contractData.Equipment = contract.Equipment;
            }
            return contractData;
        }

        private Customer AddOrUpdateCustomerLocations(Customer customer, IList<Location> locations)
        {
            //??
            var existingEntities =
                customer.Locations.Where(
                    a => locations.Any(ca => ca.Id == a.Id || ca.AddressType == a.AddressType)).ToList();
            var entriesForDelete = customer.Locations.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            locations.ForEach(addr =>
            {
                var curAddress =
                    customer.Locations.FirstOrDefault(ca => ca.AddressType == addr.AddressType);
                if (curAddress == null)
                {
                    addr.Customer = customer;
                    customer.Locations.Add(addr);
                }
                else
                {
                    addr.Customer = customer;
                    _dbContext.Entry<Location>(addr).State = EntityState.Modified;                    
                }               
            });

            return customer;
        }

        private void AddOrUpdateContactInfo(Contract contract, ContactInfo newData)
        {
            if (contract.ContactInfo != null)
            {
                contract.ContactInfo.EmailAddress = newData.EmailAddress;
                contract.ContactInfo.HouseSize = newData.HouseSize;
                contract.ContactInfo.Phones = newData.Phones;
            }
            else
            {
                contract.ContactInfo = newData;
            }
        }

        private void AddOrUpdatePaymentInfo(Contract contract, PaymentInfo newData)
        {
            if (contract.PaymentInfo != null)
            {
                contract.PaymentInfo.PaymentType = newData.PaymentType;
                contract.PaymentInfo.PrefferedWithdrawalDate = newData.PrefferedWithdrawalDate;
                contract.PaymentInfo.AccountNumber = newData.AccountNumber;
                contract.PaymentInfo.BlankNumber = newData.BlankNumber;
                contract.PaymentInfo.TransitNumber = newData.TransitNumber;
                contract.PaymentInfo.EnbridgeGasDistributionAccount = newData.EnbridgeGasDistributionAccount;
            }
            else
            {
                contract.PaymentInfo = newData;
            }
        }

        private Customer AddOrUpdateCustomer(Customer customer)
        {
            var dbCustomer = customer.Id == 0 ? null : _dbContext.Customers.Find(customer.Id);
            if (dbCustomer == null)
            {
                dbCustomer = _dbContext.Customers.Add(customer);
            }
            else
            {
                dbCustomer.FirstName = customer.FirstName;
                dbCustomer.LastName = customer.LastName;
                dbCustomer.DateOfBirth = customer.DateOfBirth;
            }
            if (dbCustomer.Locations == null)
            {
                dbCustomer.Locations = new List<Location>();
            }
            return dbCustomer;
        }

        private bool AddOrUpdateAdditionalApplicants(Contract contract, IList<Customer> customers)
        {
            var existingEntities =
                contract.SecondaryCustomers.Where(
                    ho => customers.Any(cho => cho.Id == ho.Id)).ToList();

            var entriesForDelete = contract.SecondaryCustomers.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);
            customers.ForEach(ho =>
            {
                var customer = AddOrUpdateCustomer(ho);
                if (existingEntities.Find(e => e.Id == customer.Id) == null)
                {
                    contract.SecondaryCustomers.Add(ho);
                }
            });

            contract.LastUpdateTime = DateTime.Now;

            return true;
        }
    }
}
