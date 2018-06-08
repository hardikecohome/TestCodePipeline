using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class HelpPopUpViewModal
    {
        public int Id { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "DealerName")]
        public string DealerName { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "YourName")]
        public string YourName { get; set; }
        public bool IsPreferedContactPerson { get; set; }
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredContactPerson")]
        [StringLength(30, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[ÀàÂâÆæÇçÉéÈèÊêËëÎîÏïÔôŒœÙùÛûÜüŸÿa-zA-Z \.‘'`-]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "NameIncorrectFormat")]
        public string PreferedContactPerson { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNumber")]
        [StringLength(10, MinimumLength = 4, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "ContractNumberIncorrectFormat")]
        public string LoanNumber { get; set; }

        public SupportType SupportType { get; set; }

        [CustomRequired]
        public string HelpRequested { get; set; }
        public PreferredContactType BestWayToContact { get; set; }

        [CustomRequired]
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "PhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Phone")]
        public string Phone { get; set; }
        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public string Email { get; set; }
    }         
}