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
            var oneOwner = new OwnerViewModel
            {
                FirstName = "John",
                LastName = "Doe",
                AddressInformation = new AddressInformation
                {
                    City = "Toronto",
                    PostalCode = "54H5H",
                    Province = "BC",
                    ResidenceType = ResidenceType.Own,
                    Street = "42 test",
                    UnitNumber = "12345"
                },
                BirthDate = new DateTime(1995, 05, 02),
                CellPhone = "1234567890",
                EmailAddress = "ws@ws.com",
                PercentOwnership = 50
            };
            //Owners = new List<OwnerViewModel>() { new OwnerViewModel() { AddressInformation = new AddressInformation()}};
            Owners = new List<OwnerViewModel>() { oneOwner };
            ProductInfo = new ProductInfoViewModel();
        }

        public int Id { get; set; }

        public string AccessKey { get; set; }

        public DealerOnboardingDictionariesViewModel DictionariesData { get; set; }

        public string DealerHash { get; set; }

        public bool AllowCommunicate { get; set; }

        public CompanyInfoViewModel CompanyInfo { get; set; }

        [MinMaxListCount(1,5)]
        public List<OwnerViewModel> Owners { get; set; }

        public ProductInfoViewModel ProductInfo { get; set; }
    }

    
}