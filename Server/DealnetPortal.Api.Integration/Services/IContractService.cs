using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IContractService
    {
        ContractDTO CreateContract(string contractOwnerId);
    }
}
