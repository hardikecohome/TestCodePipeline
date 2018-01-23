using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models
{
    public class InstallationPackageInformation
    {
        public int Id { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Description")]
        public string Description { get; set; }

        [CustomRequired]
        [RegularExpression(@"^[1-9]\d{0,6}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "MonthlyCostIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "MonthlyCostOfOwnership")]
        public decimal? MonthlyCost { get; set; }
    }
}