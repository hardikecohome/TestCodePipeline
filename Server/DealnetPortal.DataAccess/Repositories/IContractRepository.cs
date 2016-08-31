using System.Collections.Generic;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
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
        Contract UpdateContract(Contract contract);

        /// <summary>
        /// Update contract data
        /// </summary>
        /// <param name="contractData">Structure with data related to contract</param>
        /// <returns>Result of update</returns>
        bool UpdateContractData(ContractData contractData);

        /// <summary>
        /// Get contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Contract</returns>
        Contract GetContract(int contractId);

        /// <summary>
        /// Get contract as untracked from DB
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Contract</returns>
        Contract GetContractAsUntracked(int contractId);
    }
}
