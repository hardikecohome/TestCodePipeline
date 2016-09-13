using System;
using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    /// <summary>
    /// Helper service for work with contracts that integrate DB and 3rd party services requests
    /// </summary>
    public interface IContractService
    {
        ContractDTO CreateContract(string contractOwnerId);

        IList<ContractDTO> GetContracts(string contractOwnerId);

        ContractDTO GetContract(int contractId);

        IList<Alert> UpdateContractClientData(int contractId, IList<LocationDTO> addresses, IList<CustomerDTO> customers);

        IList<Alert> UpdateContractData(ContractDTO contract);

        IList<Alert> InitiateCreditCheck(int contractId);

        Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId);

        IList<Alert> SubmitContract(int contractId);

        IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId, FlowingSummaryType summaryType);
    }
}
