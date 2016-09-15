using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;

    /// <summary>
    /// Service agent for communicate with server-side service and controller for processing contracts (deals)
    /// </summary>
    public interface IContractServiceAgent
    {
        /// <summary>
        /// Create a new contract (deal) by current logged in user (dealer)
        /// </summary>
        /// <returns>A new contract record with a new contract Id and alert list in a tuple</returns>
        Task<Tuple<ContractDTO, IList<Alert>>> CreateContract();

        /// <summary>
        /// Get contract record by container id
        /// </summary>
        /// <param name="contractId">Container id</param>
        /// <returns>A  contract record with and alert list in a tuple</returns>
        Task<Tuple<ContractDTO, IList<Alert>>> GetContract(int contractId);

        /// <summary>
        /// Get contracts (deals) list for a current logged in user
        /// </summary>
        /// <returns>List of contracts</returns>
        Task<IList<ContractDTO>> GetContracts();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contractData"></param>
        /// <returns></returns>
        Task<IList<Alert>> UpdateContractData(ContractDataDTO contractData);

        Task<IList<Alert>> UpdateEquipmentInformation(EquipmentInformationDTO equipmentInfo);

        /// <summary>
        /// Initiate credit check for contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>List of alerts (warnings, errors)</returns>
        Task<IList<Alert>> InitiateCreditCheck(int contractId);

        /// <summary>
        /// Get credit check results for contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Credit check result and list of alerts in a tuple</returns>
        Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId);

        Task<IList<FlowingSummaryItemDTO>> GetContractsSummary(string summaryType);
    }
}
