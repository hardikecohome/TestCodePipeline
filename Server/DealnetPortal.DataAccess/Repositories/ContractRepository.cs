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
                .Include(c => c.Locations)
                .Include(c => c.ContractCustomers)
                .FirstOrDefault(c => c.Id == contractId);
        }

        public Contract GetContractAsUntracked(int contractId)
        {
            return _dbContext.Contracts
                .Include(c => c.ContractCustomers)
                .Include(c => c.Locations)
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
                //_dbContext.Customers.RemoveRange(contract.Customers.ToList());
                contract.ContractCustomers.Clear();

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
                contract.Locations.ForEach(a => _dbContext.Entry(a).State = EntityState.Deleted);
                contract.ContractCustomers.Clear();
                //contract.Customers.ForEach(ho => _dbContext.Entry(ho).State = EntityState.Deleted);
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

        public bool UpdateContractData(ContractData contractData)
        {
            bool updated = false;            
            if (contractData != null)
            {
                var contract = GetContract(contractData.Id);
                if (contract != null)
                {
                    if (contractData.Locations != null)
                    {
                        AddOrUpdateContractAddresses(contract, contractData.Locations);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                        updated = true;
                    }
                    if (contractData.Customers != null)
                    {
                        AddOrUpdateContractHomeOwners(contract, contractData.Customers);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        contract.LastUpdateTime = DateTime.Now;
                        updated = true;
                    }
                }
            }
            return updated;
        }

        public Contract UpdateContractClientData(int contractId, IList<Location> locations, IList<ContractCustomer> customers)
        {
            var contract = GetContract(contractId);
            if (locations != null)
            {
                AddOrUpdateContractAddresses(contract, locations);
                contract.ContractState = ContractState.CustomerInfoInputted;
            }
            if (customers != null)
            {
                AddOrUpdateContractHomeOwners(contract, customers);
                contract.ContractState = ContractState.CustomerInfoInputted;
            }

            return contract;
        }        

        public ContractData GetContractData(int contractId)
        {
            ContractData contractData = new ContractData()
            {
                Id = contractId
            };
            var contract = GetContractAsUntracked(contractId);
            contractData.Locations = contract.Locations.ToList();
            contractData.Customers = contract.ContractCustomers.ToList();       

            return contractData;
        }

        private Contract AddOrUpdateContractAddresses(Contract contract, IList<Location> locations)
        {
            var existingEntities =
                contract.Locations.Where(
                    a => locations.Any(ca => ca.Id == a.Id || ca.AddressType == a.AddressType)).ToList();
            var entriesForDelete = contract.Locations.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            locations.ForEach(addr =>
            {                
                var curAddress =
                    contract.Locations.FirstOrDefault(ca => ca.AddressType == addr.AddressType);
                if (curAddress == null)
                {
                    curAddress = new Location();
                    contract.Locations.Add(curAddress);
                }
                curAddress.AddressType = addr.AddressType;
                curAddress.City = addr.City;
                curAddress.PostalCode = addr.PostalCode;
                curAddress.Street = addr.Street;
                curAddress.State = addr.State;
                curAddress.Unit = addr.Unit;
                curAddress.Contract = contract;
            });

            contract.LastUpdateTime = DateTime.Now;

            return contract;
        }

        private Contract AddOrUpdateContractHomeOwners(Contract contract, IList<ContractCustomer> contractCustomers)
        {
            //var existingEntities =
            //    contract.ContractCustomers.Where(
            //        c => contractCustomers.Any(cho => cho.CustomerId == c.CustomerId)).ToList();

            //var entriesForDelete = contract.ContractCustomers.Except(existingEntities).ToList();
            //entriesForDelete.ForEach(d => contract.ContractCustomers.Remove(d));

            // clean existing contract customers entries
            contract.ContractCustomers.Clear();

            contractCustomers.ForEach(cc =>
            {
                // update customer - extract ?
                cc.Contract = contract;
                var customer = _dbContext.Customers.FirstOrDefault(c => c.Id == cc.CustomerId);
                if (customer == null)
                {
                    customer = cc.Customer;
                    _dbContext.Customers.AddOrUpdate(customer);                    
                }
                else
                {
                    customer = cc.Customer;
                    customer.Contract = contract;                    
                }
                contract.ContractCustomers.Add(cc);
                // update contractCustomer data
            });

            contract.LastUpdateTime = DateTime.Now;

            return contract;
        }

        private bool AddOrUpdateCustomers(IList<Customer> customers)
        {
            throw new NotImplementedException();
        }        
    }
}
