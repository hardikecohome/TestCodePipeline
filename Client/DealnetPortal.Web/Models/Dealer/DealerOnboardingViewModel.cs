using System;
using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Infrastructure;
using ResidenceType = DealnetPortal.Web.Models.Enumeration.ResidenceType;

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

        public int Id { get; set; }

        public string AccessKey { get; set; }

        public DealerOnboardingDictionariesViewModel DictionariesData { get; set; }

        public string OnBoardingLink { get; set; }

        public bool AllowCommunicate { get; set; }

        public CompanyInfoViewModel CompanyInfo { get; set; }

        [MinMaxListCount(1,5)]
        public List<OwnerViewModel> Owners { get; set; }

        public ProductInfoViewModel ProductInfo { get; set; }
    }
}