﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class ContractSalesRepRoleDTO
    {
        public bool InitiatedContact { get; set; }

        public bool NegotiatedAgreement { get; set; }

        public bool ConcludedAgreement { get; set; }
    }
}
