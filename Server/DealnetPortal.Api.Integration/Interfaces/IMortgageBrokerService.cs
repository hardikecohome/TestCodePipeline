using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Interfaces
{
    public interface IMortgageBrokerService
    {
        Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer);
    }
}
