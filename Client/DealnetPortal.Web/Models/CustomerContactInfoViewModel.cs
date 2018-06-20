using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models
{
    public class CustomerContactInfoViewModel : ContactInfoViewModel
    {
        [CustomRequired]
        [StringLength(256, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EmailAddress")]
        [EmailAddress(ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "InvalidEmailAddress")]
        public override string EmailAddress { get; set; }
    }
}
