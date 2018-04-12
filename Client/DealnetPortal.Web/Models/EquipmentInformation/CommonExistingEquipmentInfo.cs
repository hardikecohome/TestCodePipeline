using System.ComponentModel.DataAnnotations;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class CommonExistingEquipmentInfo
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomerOwned")]
        public bool CustomerOwned { get; set; }

        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "NameOfExistingSupplier")]
        public string RentalCompany { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ResponsibleForCostRemoval")]
        public ResponsibleForRemoval? ResponsibleForRemoval { get; set; }

        [StringLength(20, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "Other")]
        public string ResponsibleForRemovalValue { get; set; }
    }
}