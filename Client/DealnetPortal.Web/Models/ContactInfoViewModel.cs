using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Web.Models
{
    public class ContactInfoViewModel
    {
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "HomePhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "HomePhone")]
        public string HomePhone { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CellPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CellPhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "CellPhone")]
        public string CellPhone { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "BusinessPhoneMustBeLong")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "BusinessPhoneIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "BusinessPhone")]
        public string BusinessPhone { get; set; }
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public virtual string EmailAddress { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public bool AllowCommunicate { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredContactMethod")]
        public PreferredContactMethod? PreferredContactMethod { get; set; }

        public int CustomerId { get; set; }
    }
}