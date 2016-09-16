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
            var contracts = _dbContext.Contracts.Where(c => c.Dealer.Id == ownerUserId).ToList();
            return contracts;
        }

        public Contract UpdateContractState(int contractId, ContractState newState)
        {
            var contract = GetContract(contractId);
            contract.ContractState = newState;
            contract.LastUpdateTime = DateTime.Now;
            return contract;
        }

        public Contract GetContract(int contractId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.PrimaryCustomer.Phones)
                .Include(c => c.SecondaryCustomers)
                .FirstOrDefault(c => c.Id == contractId);
        }

        public Contract GetContractAsUntracked(int contractId)
        {
            return _dbContext.Contracts
                .Include(c => c.PrimaryCustomer)
                .Include(c => c.PrimaryCustomer.Locations)
                .Include(c => c.PrimaryCustomer.Phones)
                .Include(c => c.SecondaryCustomers)
                .Include(c=>c.Equipment)
                .AsNoTracking().
                FirstOrDefault(c => c.Id == contractId);
        }

        public bool DeleteContract(string contractOwnerId, int contractId)
        {
            bool deleted = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => c.Dealer.Id == contractOwnerId && c.Id == contractId);
            if (contract != null)
            {
                //remove clients for contract?
                contract.SecondaryCustomers.Clear();
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

        public Contract UpdateContract(Contract contract)
        {
            contract.ContractState = ContractState.CustomerInfoInputted;
            _dbContext.Entry(contract).State = EntityState.Modified;
            contract.LastUpdateTime = DateTime.Now;
            return contract;
        }

        public Contract UpdateContractData(ContractData contractData)
        {
            if (contractData != null)
            {
                var contract = GetContract(contractData.Id);
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

                    if (contractData.Phones != null)
                    {
                    }

                    

                    if (contractData.Equipment != null)
                    {
                        this.AddOrUpdateEquipment(contractData.Equipment);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                    }

                    return contract;
                }
            }
            return null;
        }

        private bool AddOrUpdateEquipment(EquipmentInfo contractDataEquipment)
        {
            this._dbContext.EquipmentInfo.AddOrUpdate(contractDataEquipment);
            foreach (var newEquipment in contractDataEquipment.NewEquipment)
            {
                newEquipment.EquipmentInfoId = contractDataEquipment.Id;
                this._dbContext.NewEquipment.AddOrUpdate(newEquipment);
            }
            foreach (var existingEquipment in contractDataEquipment.ExistingEquipment)
            {
                existingEquipment.EquipmentInfoId = contractDataEquipment.Id;
                this._dbContext.ExistingEquipment.AddOrUpdate(existingEquipment);
            }
            return true;
        }

        //public Contract UpdateContractClientData(int contractId, IList<Location> locations, IList<ContractCustomer> customers)
        //{
        //    var contract = GetContract(contractId);
        //    if (locations != null)
        //    {
        //        AddOrUpdateContractAddresses(contract, locations);
        //        contract.ContractState = ContractState.CustomerInfoInputted;
        //    }
        //    if (customers != null)
        //    {
        //        AddOrUpdateContractHomeOwners(contract, customers);
        //        contract.ContractState = ContractState.CustomerInfoInputted;
        //    }

        //    return contract;
        //}        

        public ContractData GetContractData(int contractId)
        {
            ContractData contractData = new ContractData()
            {
                Id = contractId
            };
            var contract = GetContractAsUntracked(contractId);
            contractData.Locations = contract.PrimaryCustomer?.Locations?.ToList();
            contractData.SecondaryCustomers = contract.SecondaryCustomers?.ToList();

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
                    curAddress = new Location();
                    customer.Locations.Add(curAddress);
                }
                curAddress.AddressType = addr.AddressType;
                curAddress.City = addr.City;
                curAddress.PostalCode = addr.PostalCode;
                curAddress.Street = addr.Street;
                curAddress.State = addr.State;
                curAddress.Unit = addr.Unit;
                curAddress.Customer = customer;
            });

            return customer;
        }

        private Customer AddOrUpdateCustomer(Customer customer)
        {
            var dbCustomer = _dbContext.Customers.Find(customer.Id);
            if (dbCustomer == null || customer.Id == 0)
            {
                dbCustomer = _dbContext.Customers.Add(customer);
            }
            else
            {
                dbCustomer.FirstName = customer.FirstName;
                dbCustomer.LastName = customer.LastName;
                dbCustomer.DateOfBirth = customer.DateOfBirth;
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
