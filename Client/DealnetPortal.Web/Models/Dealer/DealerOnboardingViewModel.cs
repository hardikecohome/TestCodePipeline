using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class DealerOnboardingViewModel
    {
        public DealerOnboardingViewModel()
        {
            CompanyInfo = new CompanyInfoViewModel();
            Owners = new List<OwnerViewModel>() { new OwnerViewModel() { AddressInformation = new AddressInformation()}};
            ProductInfo = new ProductInfoViewModel();
        }

        public DealerOnboardingDictionariesViewModel DictionariesData { get; set; }

        public string DealerHash { get; set; }

        public bool AllowCommunicate { get; set; }

        public CompanyInfoViewModel CompanyInfo { get; set; }

        [MinMaxListCount(1,5)]
        public List<OwnerViewModel> Owners { get; set; }

        public ProductInfoViewModel ProductInfo { get; set; }
    }

    
}