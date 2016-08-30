using System.Collections.Generic;
using DealnetPortal.Api.Models;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface IContractRepository
    {
        Contract CreateContract(string contractOwnerId);

        IList<Contract> GetContracts(string ownerUserId);

        bool DeleteContract(string contractOwnerId, int contractId);

        bool CleanContract(string contractOwnerId, int contractId);

        Contract UpdateContract(Contract contract);

        bool UpdateContractData(ContractData contractData);

        Contract GetContract(int contractId);
    }
}
