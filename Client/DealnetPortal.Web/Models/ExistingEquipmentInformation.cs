using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.EquipmentInformation
{    
    public class ExistingEquipmentInformation
    {
        [Required]
        [Display(Name= "Dealer Is Replacing?")]
        public bool DealerIsReplacing  { get; set; }

        [Required]
        [Display(Name = "Is Rental?")]
        public bool IsRental { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Rental Company")]
        public string RentalCompany { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Estimated Age")]
        public string EstimatedAge { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [StringLength(50)]
        [Display(Name = "Serial Number")]
        public string SerialNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "General Condition")]
        public string GeneralCondition { get; set; }

        [StringLength(100)]
        public string Notes { get; set; }       
    }
}