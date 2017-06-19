using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models
{
    public class CreditRejectedViewModel
    {
        public int ContractId { get; set; }
        public bool CanAddAdditionalApplicants { get; set; }
    }
}