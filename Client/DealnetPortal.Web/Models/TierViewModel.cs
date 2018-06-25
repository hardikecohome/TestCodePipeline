using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class TierViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool PassAdminFee { get; set; }

        public List<RateCardViewModel> RateCards { get; set; }

        public List<ReductionCardViewModel> RateReductionCards { get; set; }

        public CustomerRiskGroupViewModel CustomerRiskGroup { get; set; }

    }
}