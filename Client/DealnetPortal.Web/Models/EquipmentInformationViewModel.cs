using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class EquipmentInformationViewModel
    {
        [Display(Name = "Type of agreement")]
        public AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][1-9]?)?$")]
        [Display(Name = "Total Monthly Payment")]
        public double? TotalMonthlyPayment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]+$")]
        [Display(Name = "Requested Term")]
        public int? RequestedTerm { get; set; }
        
        [RegularExpression(@"^[0-9]+$")]
        [Display(Name = "Amortization Term")]
        public int? AmortizationTerm { get; set; }

        [Display(Name = "Customer Rate (%)")]
        public double? CustomerRate { get; set; }

        [Display(Name = "Admin Fee")]
        public double? AdminFee { get; set; }

        [Display(Name = "Down Payment")]
        public double? DownPayment { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Sales Rep")]
        [RegularExpression(@"^[^0-9]+$")]
        public string SalesRep { get; set; }

        [StringLength(500)]
        [Display(Name = "Contract Notes")]
        public string Notes { get; set; }

        public int? ContractId { get; set; }

        public double ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }
    }
}