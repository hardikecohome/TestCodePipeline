﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Storage;

namespace DealnetPortal.Api.Integration.Interfaces
{
    /// <summary>
    /// Helper service for work with contracts that integrate DB and 3rd party services requests
    /// </summary>
    public interface IContractService
    {
        ContractDTO CreateContract(string contractOwnerId);

        IList<ContractShortInfoDTO> GetContractsShortInfo(string contractOwnerId);

        IList<ContractDTO> GetContracts(string contractOwnerId);

        int GetCustomersContractsCount(string contractOwnerId);

        IList<ContractDTO> GetContracts(IEnumerable<int> ids, string ownerUserId);

        IList<ContractDTO> GetDealerLeads(string userId);

        ContractDTO GetContract(int contractId, string contractOwnerId);

        IList<Alert> UpdateContractData(ContractDataDTO contract, string contractOwnerId, ContractorDTO contractor = null);

        IList<Alert> NotifyContractEdit(int contractId, string contractOwnerId);

        AgreementDocument  GetContractsFileReport(IEnumerable<int> ids, string contractOwnerId, int? timeZoneOffset = null);

        IList<Alert> UpdateInstallationData(InstallationCertificateDataDTO installationCertificateData, string contractOwnerId);
        
        Tuple<CreditCheckDTO, IList<Alert>> SubmitContract(int contractId, string contractOwnerId);

        IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId, FlowingSummaryType summaryType);

        Tuple<IList<DocumentTypeDTO>, IList<Alert>> GetContractDocumentTypes(int contractId, string contractOwnerId);

        Tuple<int?, IList<Alert>> AddDocumentToContract(ContractDocumentDTO document, string contractOwnerId);

        IList<Alert> RemoveContractDocument(int documentId, string contractOwnerId);

        Task<IList<Alert>> SubmitAllDocumentsUploaded(int contractId, string contractOwnerId);

        Tuple<IList<EquipmentTypeDTO>, IList<Alert>> GetDealerEquipmentTypes(string dealerId);                

        Tuple<ProvinceTaxRateDTO, IList<Alert>> GetProvinceTaxRate(string province);

        CustomerDTO GetCustomer(int customerId);

        IList<Alert> UpdateCustomers(CustomerDataDTO[] customers, string contractOwnerId);

        Tuple<int?, IList<Alert>> AddComment(CommentDTO comment, string contractOwnerId);

        IList<Alert> RemoveComment(int commentId, string contractOwnerId);        

        IList<Alert> RemoveContract(int documentId, string contractOwnerId);

        Task<IList<Alert>> AssignContract(int contractId, string newContractOwnerId);
    }
}
