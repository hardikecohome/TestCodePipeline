using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class CompanyInfoViewModel
    {
        [CustomRequired]
        [Display(ResourceType =typeof(Resources.Resources),Name ="LegalName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LegalNameIncorrectFormat")]
        public string LegalName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "OperatingName")]
        [StringLength(20, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "OperatingNameIncorrectFormat")]
        public string OperatingName { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        public string Phone { get; set; }

        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string EmailAddress { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "Site")]
        [DataType(DataType.Url, ErrorMessageResourceType =typeof(Resources.Resources),ErrorMessageResourceName = "SiteInvalidFormat")]
        public string Site { get; set; }

        public AddressInformation AddressInfo { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "YearsInBusinessMax3")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "YearsInBusinessIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "YearsInBusiness")]
        public int YearsInBusiness { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NumInstallersMax3")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NumInstallersIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumInstallers")]
        public int NumberOfInstallers { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NumSalesMax3")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NumSalesIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumSales")]
        public int NumberOfSales { get; set; }

        [MinListCount(1, ErrorMessageResourceName = "ProvincesMinLength", ErrorMessageResourceType =typeof(Resources.Resources))]
        [Display(ResourceType =typeof(Resources.Resources), Name = "Provinces")]
        public List<string> Provinces { get; set; }
    }
}