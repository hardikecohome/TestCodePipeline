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

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Total Monthly Payment is in incorrect format")]
        [Display(Name = "Total Monthly Payment")]
        public double? TotalMonthlyPayment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Requested Term is in incorrect format")]
        [Display(Name = "Requested Term")]
        public int? RequestedTerm { get; set; }
        
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Amortization Term is in incorrect format")]
        [Display(Name = "Amortization Term")]
        public int? AmortizationTerm { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Customer Rate is in incorrect format")]
        [Display(Name = "Customer Rate (%)")]
        public double? CustomerRate { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Admin Fee is in incorrect format")]
        [Display(Name = "Admin Fee")]
        public double? AdminFee { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessage = "Down Payment is in incorrect format")]
        [Display(Name = "Down Payment")]
        public double? DownPayment { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Sales Rep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessage = "Sales Rep is in incorrect format")]
        public string SalesRep { get; set; }

        [StringLength(500)]
        [Display(Name = "Contract Notes")]
        public string Notes { get; set; }

        public int? ContractId { get; set; }

        public double ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }
    }
}