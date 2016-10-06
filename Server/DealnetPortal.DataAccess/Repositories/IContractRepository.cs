using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// An interface for Contracts repository in DB
    /// </summary>
    public interface IContractRepository
    {
        /// <summary>
        /// Create a new contract (deal)
        /// </summary>
        /// <param name="contractOwnerId">user Id</param>
        /// <returns>Contract</returns>
        Contract CreateContract(string contractOwnerId);

        /// <summary>
        /// Get user contracts list
        /// </summary>
        /// <param name="ownerUserId">user Id</param>
        /// <returns>List of contracts</returns>
        IList<Contract> GetContracts(string ownerUserId);

        /// <summary>
        /// Get list of user contracts with particular ids
        /// </summary>
        /// <param name="ids">List od Ids</param>
        /// <param name="ownerUserId">user Id</param>
        /// <returns>List of contracts</returns>
        IList<Contract> GetContracts(IEnumerable<int> ids, string ownerUserId);

        /// <summary>
        /// Delete contract with all linked data
        /// </summary>
        /// <param name="contractOwnerId">contract owner user Id</param>
        /// <param name="contractId">contract Id</param>
        /// <returns>Result of delete</returns>
        bool DeleteContract(string contractOwnerId, int contractId);

        /// <summary>
        /// Clean all data linked with contract, except contract general info
        /// </summary>
        /// <param name="contractOwnerId">contract owner user Id</param>
        /// <param name="contractId">contract Id</param>
        /// <returns>Result of clean</returns>
        bool CleanContract(string contractOwnerId, int contractId);

        /// <summary>
        /// Update contract entity in DB
        /// </summary>
        /// <param name="contract">Contract</param>
        /// <returns>Contract</returns>
        Contract UpdateContract(Contract contract, string contractOwnerId);

        /// <summary>
        /// Update contract data
        /// </summary>
        /// <param name="contractData">Structure with data related to contract</param>
        /// <returns>Result of update</returns>
        Contract UpdateContractData(ContractData contractData, string contractOwnerId);

        /// <summary>
        /// Update contract customers data
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="addresses">Updated addresses list, or null</param>
        /// <param name="customers">Updated cutomers list, or null</param>
        /// <returns>Updated contract record</returns>
        //Contract UpdateContractPrimaryClientData(int contractId, IList<Location> locations, IList<Phone> phones);

        /// <summary>
        /// Update contract state
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <param name="newState">A new state of contract</param>
        /// <returns>Updated contract record</returns>
        Contract UpdateContractState(int contractId, string contractOwnerId, ContractState newState);

        /// <summary>
        /// Get contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Contract</returns>
        Contract GetContract(int contractId, string contractOwnerId);

        /// <summary>
        /// Get contract as untracked from DB
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Contract</returns>
        Contract GetContractAsUntracked(int contractId, string contractOwnerId);

        /// <summary>
        /// Get Equipment Types list
        /// </summary>
        /// <returns>List of Equipment Type</returns>
        IList<EquipmentType> GetEquipmentTypes();

        /// <summary>
        /// Get Province Tax Rate
        /// </summary>
        /// <param name="province">Province abbreviation</param>
        /// <returns>Tax Rate for particular Province</returns>
        ProvinceTaxRate GetProvinceTaxRate(string province);
    }
}
