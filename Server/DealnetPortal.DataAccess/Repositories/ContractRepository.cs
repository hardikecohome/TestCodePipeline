using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using DealnetPortal.Api.Models;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Enums;

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

        public Contract UpdateContract(Contract contract)
        {
            contract.ContractState = ContractState.InProgress;
            //_dbContext.Entry(contract).State = EntityState.Modified;
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
                        ContractAddress address;
                        if (contract.ContractAddress != null)
                        {
                            address = contract.ContractAddress;
                            //_dbContext.Entry(address).State = EntityState.Modified;                        
                        }
                        else
                        {
                            address = new ContractAddress()
                            {
                                Contract = contract
                            };
                            //_dbContext.ContractAddresses.Add(address);
                        }
                        address.City = contractData.ContractAddress.City;
                        address.PostalCode = contractData.ContractAddress.PostalCode;
                        address.Street = contractData.ContractAddress.Street;
                        address.Province = contractData.ContractAddress.Province;
                        address.Unit = contractData.ContractAddress.Unit;
                        //_dbContext.ContractAddresses.AddOrUpdate(address);

                        updated = true;
                    }
                    if (contractData.HomeOwners != null && contractData.HomeOwners.Any())
                    {
                        //???                    
                    }
                }
            }
            return updated;
        }        

        public Contract GetContract(int contractId)
        {
            return _dbContext.Contracts.FirstOrDefault(c => c.Id == contractId);
        }

        public ContractData GetContractData(int contractId)
        {
            throw new System.NotImplementedException();
        }
    }
}
