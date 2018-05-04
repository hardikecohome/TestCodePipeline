using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class SalesRepInformation
    {
        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        public bool IniatedContract { get; set; }
        public bool NegotiatedAgreement { get; set; }
        public bool ConcludedAgreement { get; set; }
    }
}