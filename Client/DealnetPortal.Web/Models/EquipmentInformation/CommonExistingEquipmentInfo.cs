using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class CommonExistingEquipmentInfo
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "IsRental")]
        public bool IsRental { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "RentalCompany")]
        public string RentalCompany { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ResponsibleForCostRemoval")]
        public ResponsibleForRemoval? ResponsibleForRemoval { get; set; }
    }
}