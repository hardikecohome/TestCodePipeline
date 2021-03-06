﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IMortgageBrokerServiceAgent
    {
        Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(NewCustomerDTO customerForm);
        Task<IList<ContractDTO>> GetCreatedContracts();
    }
}
