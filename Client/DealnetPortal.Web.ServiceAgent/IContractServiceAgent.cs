using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Web.ServiceAgent
{
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

        Task<int> GetCustomersContractsCount();

        Task<IList<ContractDTO>> GetCompletedContracts();

        Task<IList<ContractDTO>> GetLeads();

        Task<IList<ContractDTO>> GetCreatedContracts();

        Task<Tuple<IList<ContractDTO>, IList<Alert>>> GetContracts(IEnumerable<int> ids);

        Task<IList<Alert>> NotifyContractEdit(int contractId);

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

        Task<IList<Alert>> UpdateInstallationData(InstallationCertificateDataDTO installationCertificateData);

        /// <summary>
        /// Get contract installation certificate (pdf)
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<AgreementDocument, IList<Alert>>> GetInstallationCertificate(int contractId);

        /// <summary>
        /// Check is contract print (pdf) agreement available
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<bool, IList<Alert>>> CheckContractAgreementAvailable(int contractId);

        /// <summary>
        /// Check is installation certificate (pdf) available
        /// </summary>
        /// <param name="contractId">Contract Id</param>
        /// <returns></returns>
        Task<Tuple<bool, IList<Alert>>> CheckInstallationCertificateAvailable(int contractId);

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
        Task<AgreementDocument> GetXlsxReport(IEnumerable<int> ids);

        Task<Tuple<int?, IList<Alert>>> AddComment(CommentDTO comment);

        Task<IList<Alert>> RemoveComment(int commentId);

        Task<Tuple<int?, IList<Alert>>> AddDocumentToContract(ContractDocumentDTO document);

        Task<IList<Alert>> RemoveContractDocument(int documentId);

        Task<IList<Alert>> SubmitAllDocumentsUploaded(int contractId);

        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormDTO customerForm);

        Task<CustomerContractInfoDTO> GetCustomerContractInfo(int contractId, string dealerName);

        Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(NewCustomerDTO customerForm);

        Task<IList<Alert>> RemoveContract(int contractId);

        /// <summary>
        /// Get Rates Card by Dealer
        /// </summary>
        Task<TierDTO> GetDealerTier();
        Task<TierDTO> GetDealerTier(int contractId);

        Task<IList<Alert>> AssignContract(int contractId);
        Task<IList<Alert>> CheckCustomerExisting(string email);
    }
}
