using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DealnetPortal.Web.Models.EquipmentInformation
{    
    public class ExistingEquipmentInformation
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "IsRental")]
        public bool IsRental { get; set; }

        [Required]
        [StringLength(50)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "RentalCompany")]
        public string RentalCompany { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "EstimatedAge")]
        public double EstimatedAge { get; set; }

        [Required]
        [StringLength(50)]
        public string Make { get; set; }

        [Required]
        [StringLength(50)]
        public string Model { get; set; }

        [Required]
        [StringLength(50)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SerialNumber")]
        public string SerialNumber { get; set; }

        [Required]
        [StringLength(50)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "GeneralCondition")]
        public string GeneralCondition { get; set; }

        [StringLength(500)]
        public string Notes { get; set; }

        public int Id { get; set; }
    }
}