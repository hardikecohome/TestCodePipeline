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

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Cost is in incorrect format")]
        public double? Cost { get; set; }
        
        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Monthly Cost is in incorrect format")]
        [Display(Name = "Monthly Cost")]
        public double? MonthlyCost { get; set; }

        [Display(Name = "Estimated Installation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime EstimatedInstallationDate{ get; set; }

        public int Id { get; set; }
    }
}