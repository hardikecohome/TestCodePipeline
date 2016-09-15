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
        [StringLength(3,MinimumLength = 1)]
        [RegularExpression(@"^[1-9]\d*$")]
        public int Quantity { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][1-9]?)?$")]
        public double Cost { get; set; }

        [Required]
        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][1-9]?)?$")]
        [Display(Name = "Monthly Cost")]
        public double MonthlyCost { get; set; }

        [Display(Name = "Estimated Installation Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime EstimatedInstallationDate{ get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][1-9]?)?$")]
        [Display(Name= "Total Monthly Payment")]
        public double TotalMonthlyPayment { get; set; }
    }
}