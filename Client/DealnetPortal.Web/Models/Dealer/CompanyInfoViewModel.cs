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
            CompanyAddress = new AddressInformation();
            Provinces = new List<string>();
        }

        [CustomRequired]
        [Display(ResourceType =typeof(Resources.Resources),Name ="FullLegalName")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string FullLegalName { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "OperatingName")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string OperatingName { get; set; }

        [CustomRequired]
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneNumberMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneNumberIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PhoneNumber")]
        public string Phone { get; set; }

        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string EmailAddress { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "Website")]
        [StringLength(50, MinimumLength = 3, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "WebsiteMustBeLong")]
        //[Url(ErrorMessageResourceType =typeof(Resources.Resources),ErrorMessageResourceName = "SiteInvalidFormat")]
        public string Website { get; set; }

        public AddressInformation CompanyAddress { get; set; }
        
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "YearsInBusiness")]
        public YearsInBusiness? YearsInBusiness { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumInstallers")]
        public NumberOfPeople? NumberOfInstallers { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "NumSales")]
        public NumberOfPeople? NumberOfSales { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name= "TypeOfBusiness")]
        public BusinessType? BusinessType { get; set; }
        
        [Display(ResourceType =typeof(Resources.Resources), Name = "Provinces")]
        public List<string> Provinces { get; set; }
    }
}