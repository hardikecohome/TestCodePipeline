using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models
{
    public class TierViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool PassAdminFee { get; set; }

        public List<RateCardViewModel> RateCards { get; set; }

        public CustomerRiskGroupViewModel CustomerRiskGroup { get; set; }

    }
}