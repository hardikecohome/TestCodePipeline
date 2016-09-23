using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public enum AgreementType
    {
        [Display(Name = "Loan Application")]
        LoanApplication,
        [Display(Name = " Rental Application (HWT)")]
        RentalApplicationHwt,
        [Display(Name = "Rental Application")]
        RentalApplication
    }
    public class EquipmentInformationViewModel
    {
        [Display(Name = "Type of agreement")]
        public AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][1-9]?)?$")]
        [Display(Name = "Total Monthly Payment")]
        public double TotalMonthlyPayment { get; set; }

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

        public int? ContractId { get; set; }
    }
}