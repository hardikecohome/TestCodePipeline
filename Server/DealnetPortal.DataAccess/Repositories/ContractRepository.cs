using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Security.Policy;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Enums;
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
                .Include(c => c.Customers)
                .Include(c => c.Addresses)
                .FirstOrDefault(c => c.Id == contractId);
        }

        public Contract GetContractAsUntracked(int contractId)
        {
            return _dbContext.Contracts
                .Include(c => c.Customers)
                .Include(c => c.Addresses)
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
                _dbContext.Customers.RemoveRange(contract.Customers.ToList());

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
                contract.Addresses.ForEach(a => _dbContext.Entry(a).State = EntityState.Deleted);
                contract.Customers.ForEach(ho => _dbContext.Entry(ho).State = EntityState.Deleted);
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
                    if (contractData.Addresses != null)
                    {
                        AddOrUpdateContractAddresses(contract, contractData.Addresses);
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

        public Contract UpdateContractClientData(int contractId, IList<ContractAddress> addresses, IList<Customer> customers)
        {
            var contract = GetContract(contractId);
            if (addresses != null)
            {
                AddOrUpdateContractAddresses(contract, addresses);
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
            contractData.Addresses = contract.Addresses.ToList();
            contractData.Customers = contract.Customers.ToList();       

            return contractData;
        }

        private Contract AddOrUpdateContractAddresses(Contract contract, IList<ContractAddress> contractAddresses)
        {
            var existingEntities =
                contract.Addresses.Where(
                    a => contractAddresses.Any(ca => ca.Id == a.Id || ca.AddressType == a.AddressType)).ToList();
            var entriesForDelete = contract.Addresses.Except(existingEntities).ToList();
            entriesForDelete.ForEach(e => _dbContext.Entry(e).State = EntityState.Deleted);

            contractAddresses.ForEach(addr =>
            {                
                var curAddress =
                    contract.Addresses.FirstOrDefault(ca => ca.Id == addr.Id || ca.AddressType == addr.AddressType);
                if (curAddress == null)
                {
                    curAddress = new ContractAddress();
                    contract.Addresses.Add(curAddress);
                }
                curAddress.City = addr.City;
                curAddress.PostalCode = addr.PostalCode;
                curAddress.Street = addr.Street;
                curAddress.Province = addr.Province;
                curAddress.Unit = addr.Unit;
                curAddress.Contract = contract;
            });
                       
            return contract;
        }

        private Contract AddOrUpdateContractHomeOwners(Contract contract, IList<Customer> customers)
        {
            var existingEntities =
                contract.Customers.Where(
                    ho => customers.Any(cho => cho.Id == ho.Id)).ToList();

            var entriesForDelete = contract.Customers.Except(existingEntities);
            _dbContext.Customers.RemoveRange(entriesForDelete);
            customers.ForEach(ho =>
            {
                ho.Contract = contract;
                _dbContext.Customers.AddOrUpdate(ho);
            });
                              
            return contract;
        }        
    }
}
