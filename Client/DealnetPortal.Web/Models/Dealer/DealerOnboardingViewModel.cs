using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.Dealer
{
    public class DealerOnboardingViewModel
    {
        public string DealerHash { get; set; }
        public bool AllowCommunicate { get; set; }

        public CompanyInfoViewModel CompanyInfo { get; set; }

        public List<OwnerViewModel> Owners { get; set; }

        public ProductInfoViewModel ProductInfo { get; set; }
    }
}