using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMortgageBrokerService
    {
        Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer);
    }
}
