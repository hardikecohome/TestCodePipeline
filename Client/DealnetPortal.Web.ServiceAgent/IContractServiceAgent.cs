using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;

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

        Task<IList<ContractDTO>> GetCompletedContracts();

        Task<Tuple<IList<ContractDTO>, IList<Alert>>> GetContracts(IEnumerable<int> ids);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="contractData"></param>
        /// <returns></returns>
        Task<IList<Alert>> UpdateContractData(ContractDataDTO contractData);

        Task<IList<Alert>> UpdateCustomerData(CustomerDataDTO[] customers);

        //Task<IList<Alert>> UpdateEquipmentInformation(EquipmentInformationDTO equipmentInfo);

        /// <summary>
        /// Initiate credit check for contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>List of alerts (warnings, errors)</returns>
        Task<IList<Alert>> InitiateCreditCheck(int contractId);

        /// <summary>
        /// Initiate a process of digital signature of contract
        /// </summary>
        /// <param name="signatureUsers"></param>
        /// <returns></returns>
        Task<IList<Alert>> InitiateDigitalSignature(SignatureUsersDTO signatureUsers);

        /// <summary>
        /// Get credit check results for contract
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns>Credit check result and list of alerts in a tuple</returns>
        Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId);

        /// <summary>
        /// Get contract print (pdf) agreement
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId);

        /// <summary>
        /// Check is contract print (pdf) agreement available
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<bool, IList<Alert>>> CheckContractAgreementAvailable(int contractId);

        /// <summary>
        /// Reports info about deals flowing
        /// </summary>
        /// <param name="summaryType"></param>
        /// <returns></returns>
        Task<IList<FlowingSummaryItemDTO>> GetContractsSummary(string summaryType);

        /// <summary>
        /// Submit contract and send it to Aspire
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<CreditCheckDTO, IList<Alert>>> SubmitContract(int contractId);

        /// <summary>
        /// Get xlsx report
        /// </summary>
        /// <param name="ids">List od Ids</param>
        /// <returns>xlsx report in byte array</returns>
        Task<byte[]> GetXlsxReport(IEnumerable<int> ids);

        Task<Tuple<int?, IList<Alert>>> AddComment(CommentDTO comment);

        Task<IList<Alert>> RemoveComment(int commentId);

        Task<Tuple<int?, IList<Alert>>> AddDocumentToContract(ContractDocumentDTO document);

        Task<IList<Alert>> RemoveContractDocument(int documentId);
    }
}
