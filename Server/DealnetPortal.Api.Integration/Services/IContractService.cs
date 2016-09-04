using System.Collections.Generic;
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

        bool UpdateContractClientData(int contractId, IList<ContractAddressDTO> addresses, IList<CustomerDTO> customers);

        bool InitiateCreditCheck(int contractId);

        bool GetCreditCheckResult(int contractId);

        bool SubmitContract(int contractId);
    }
}
