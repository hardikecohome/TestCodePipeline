using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.EquipmentInformation
{    
    public class ExistingEquipmentInformation
    {
        [Display(Name = "Is Rental?")]
        public bool IsRental { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Rental Company")]
        public string RentalCompany { get; set; }

        [Display(Name = "Estimated Age")]
        public double EstimatedAge { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "General Condition")]
        public string GeneralCondition { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }       
    }
}