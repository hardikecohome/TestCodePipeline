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

        public Contract GetContract(int contractId)
        {
            return _dbContext.Contracts.Include(c => c.Customers).
                FirstOrDefault(c => c.Id == contractId);
        }

        public Contract GetContractAsUntracked(int contractId)
        {
            return _dbContext.Contracts.Include(c => c.Customers).AsNoTracking().
                FirstOrDefault(c => c.Id == contractId);
        }

        public bool DeleteContract(string contractOwnerId, int contractId)
        {
            bool deleted = false;
            var contract = _dbContext.Contracts.FirstOrDefault(c => c.Dealer.Id == contractOwnerId && c.Id == contractId);
            if (contract != null)
            {
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
                contract.ContractAddress = null;
                contract.Customers.ForEach(ho => _dbContext.Entry(ho).State = EntityState.Deleted);
                cleaned = true;
            }
            return cleaned;
        }

        public Contract UpdateContract(Contract contract)
        {
            contract.ContractState = ContractState.CustomerInfoInputted;
            _dbContext.Entry(contract).State = EntityState.Modified;
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
                    if (contractData.ContractAddress != null)
                    {
                        AddOrUpdateContractAddress(contract, contractData.ContractAddress);
                        contract.ContractState = ContractState.CustomerInfoInputted;                        
                        updated = true;
                    }
                    if (contractData.Customers != null)
                    {
                        AddOrUpdateContractHomeOwners(contract, contractData.Customers);
                        contract.ContractState = ContractState.CustomerInfoInputted;
                        updated = true;
                    }
                }
            }
            return updated;
        }

        public Contract UpdateContractClientData(int contractId, ContractAddress contractAddress, IList<Customer> customers)
        {
            var contract = GetContract(contractId);
            if (contractAddress != null)
            {
                AddOrUpdateContractAddress(contract, contractAddress);
            }
            if (customers != null)
            {
                AddOrUpdateContractHomeOwners(contract, customers);
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
            contractData.ContractAddress = contract.ContractAddress;
            contractData.Customers = contract.Customers.ToList();       

            return contractData;
        }

        private Contract AddOrUpdateContractAddress(Contract contract, ContractAddress contractAddress)
        {
            ContractAddress address;
            if (contract.ContractAddress != null)
            {
                address = contractAddress;
                _dbContext.Entry(address).State = EntityState.Modified;                        
            }
            else
            {
                address = new ContractAddress()
                {
                    Contract = contract
                };
            }
            address.City = contractAddress.City;
            address.PostalCode = contractAddress.PostalCode;
            address.Street = contractAddress.Street;
            address.Province = contractAddress.Province;
            address.Unit = contractAddress.Unit;
            contract.ContractAddress = address;
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
