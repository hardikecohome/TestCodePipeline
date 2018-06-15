using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IMortgageBrokerServiceAgent
    {
        Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(NewCustomerDTO customerForm);
        Task<IList<ContractDTO>> GetCreatedContracts();

        Task<IList<Alert>> CheckCustomerExisting(string email);
    }
}
