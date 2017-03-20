using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.AccessControl;

    public class NewEquipmentInformation
    {
        [Required]
        public string Type { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "Type")]
        public string TypeDescription { get; set; }

        [Required]
        [StringLength(500)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "Cost")]
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CostIncorrectFormat")]
        public double? Cost { get; set; }

        [Required]
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "MonthlyCostIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "MonthlyCost")]
        public double? MonthlyCost { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate{ get; set; }

        public int Id { get; set; }
    }
}