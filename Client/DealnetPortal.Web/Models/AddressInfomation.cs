using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models
{
    public class AddressInformation
    {
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Street")]
        [StringLength(100, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InstallationAddressIncorrectFormat")]
        public string Street { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "UnitNumber")]
        [StringLength(10, MinimumLength = 1, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[a-zA-Z0-9 ]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "UnitNumberIncorrectFormat")]
        public string UnitNumber { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "City")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z0-9 \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CityIncorrectFormat")]
        public string City { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Province")]
        //[StringLength(2, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ProvinceIncorrectFormat")]
        public string Province { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PostalCode")]
        [StringLength(6, MinimumLength = 5, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PostalCodeIncorrectFormat")]
        public string PostalCode { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Residence")]
        public ResidenceType ResidenceType { get; set; }
    }
}