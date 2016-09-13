namespace DealnetPortal.Web.Models.EquipmentInformation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EquipmentInformationViewModel
    {
        public List<NewEquipmentInformation> NewEquipment { get; set; }
        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [Required]
        [StringLength(10)]
        [RegularExpression(@"^[0-9\/]+$")]
        [Display(Name = "Requested Term")]
        public string RequestedTerm { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Sales Rep")]
        [RegularExpression(@"^[^0-9]+$")]
        public string SalesRep { get; set; }

        [StringLength(500)]
        [Display(Name = "Contract Notes")]
        public string Notes { get; set; }
    }
}