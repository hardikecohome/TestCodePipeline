using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.EquipmentInformation
{    
    public class ExistingEquipmentInformation
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "IsRental")]
        public bool IsRental { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalCompany")]
        public string RentalCompany { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "EstimatedAge")]
        public double EstimatedAge { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Make")]
        public string Make { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Model")]
        public string Model { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SerialNumber")]
        public string SerialNumber { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "GeneralCondition")]
        public string GeneralCondition { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Notes")]
        public string Notes { get; set; }

        public int Id { get; set; }
    }
}