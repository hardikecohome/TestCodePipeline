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
            Owners = new List<OwnerViewModel>() { new OwnerViewModel() { Address = new AddressInformation()}};
            ProductInfo = new ProductInfoViewModel();
        }

        public int Id { get; set; }

        public string AccessKey { get; set; }

        public DealerOnboardingDictionariesViewModel DictionariesData { get; set; }

        public string OnBoardingLink { get; set; }

        public bool AllowCommunicate { get; set; }

        public CompanyInfoViewModel CompanyInfo { get; set; }
        
        public List<OwnerViewModel> Owners { get; set; }

        public ProductInfoViewModel ProductInfo { get; set; }
    }

    
}