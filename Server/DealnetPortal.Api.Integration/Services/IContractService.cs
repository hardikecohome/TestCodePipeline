using System;
using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    using Models.Contract.EquipmentInformation;
    using Models.Signature;

    /// <summary>
    /// Helper service for work with contracts that integrate DB and 3rd party services requests
    /// </summary>
    public interface IContractService
    {
        ContractDTO CreateContract(string contractOwnerId);

        IList<ContractDTO> GetContracts(string contractOwnerId);

        IList<Contract> GetContracts(IEnumerable<int> ids, string ownerUserId);

        ContractDTO GetContract(int contractId, string contractOwnerId);

        IList<Alert> UpdateContractData(ContractDataDTO contract, string contractOwnerId);
        
        IList<Alert> InitiateCreditCheck(int contractId, string contractOwnerId);

        IList<Alert> InitiateDigitalSignature(int contractId, string contractOwnerId, SignatureUser[] signatureUsers);

        Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId);

        IList<Alert> SubmitContract(int contractId, string contractOwnerId);

        IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId, FlowingSummaryType summaryType);

        Tuple<IList<EquipmentTypeDTO>, IList<Alert>> GetEquipmentTypes();

        Tuple<ProvinceTaxRateDTO, IList<Alert>> GetProvinceTaxRate(string province);

        CustomerDTO GetCustomer(int customerId);

        IList<Alert> UpdateCustomers(CustomerDataDTO[] customers, string contractOwnerId);
    }
}
