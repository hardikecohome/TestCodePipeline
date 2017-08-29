using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration.Dealer;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class CompanyInfoViewModel
    {
        public CompanyInfoViewModel()
        {
            AddressInfo = new AddressInformation();
            Provinces = new List<string>();
        }

        [CustomRequired]
        [Display(ResourceType =typeof(Resources.Resources),Name ="FullLegalName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LegalNameIncorrectFormat")]
        public string FullLegalName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "OperatingName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OperatingNameIncorrectFormat")]
        public string OperatingName { get; set; }

        [CustomRequired]
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        public string Phone { get; set; }

        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string EmailAddress { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "Site")]
        [DataType(DataType.Url, ErrorMessageResourceType =typeof(Resources.Resources),ErrorMessageResourceName = "SiteInvalidFormat")]
        public string Website { get; set; }

        public AddressInformation AddressInfo { get; set; }
        
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "YearsInBusiness")]
        public YearsInBusiness YearsInBusiness { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumInstallers")]
        public NumberOfPeople? NumberOfInstallers { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumSales")]
        public NumberOfPeople? NumberOfSales { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name= "TypeOfBusiness")]
        public BusinessType BusinessType { get; set; }

        [MinListCount(1, ErrorMessageResourceName = "ProvincesMinLength", ErrorMessageResourceType =typeof(Resources.Resources))]
        [Display(ResourceType =typeof(Resources.Resources), Name = "Provinces")]
        public List<string> Provinces { get; set; }
    }
}