using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    using System.ComponentModel.DataAnnotations;
    using System.Security.AccessControl;

    public class NewEquipmentInformation
    {
        [CustomRequired]
        public string Type { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "Type")]
        public string TypeDescription { get; set; }

        [CustomRequired]
        [StringLength(500)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "Description")]
        public string Description { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "Cost")]
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CostIncorrectFormat")]
        public double? Cost { get; set; }

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