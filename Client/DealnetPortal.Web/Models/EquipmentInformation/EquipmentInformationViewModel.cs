using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class EquipmentInformationViewModel
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        [Display(ResourceType =typeof(Resources.Resources), Name = "ProgramType")]
        public AnnualEscalationType? RentalProgramType { get; set; }

        public CustomerRiskGroupViewModel CustomerRiskGroup { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }
        public List<InstallationPackageInformation> InstallationPackages { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TotalMonthlyPaymentIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "TotalMonthlyPaymentWithoutTaxes")]
        public double? TotalMonthlyPayment { get; set; }

        public CommonExistingEquipmentInfo CommonExistingEquipmentInfo { get; set; }
        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestedTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "RequestedTerm")]
        public int? RequestedTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LoanTermMustBe3Max")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LoanTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "LoanTerm")]
        public int? LoanTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AmortizationTermMustBe3Max")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AmortizationTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AmortizationTerm")]
        public int? AmortizationTerm { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "DeferralType")]
        public LoanDeferralType LoanDeferralType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "DeferralType")]
        public RentalDeferralType RentalDeferralType { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CustomerRateIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomerRatePercentage")]
        public double? CustomerRate { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AdminFeeIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AdminFee")]
        public double? AdminFee { get; set; }

        public bool? IsFeePaidByCutomer { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "DownPaymentIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "DownPayment")]
        public double? DownPayment { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }

        public bool FullUpdate { get; set; }

        public int? ContractId { get; set; }

        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }
        public bool IsFirstStepAvailable { get; set; }
        public bool IsEditAvailable { get; set; }

        public decimal? CreditAmount { get; set; }

        public double? ValueOfDeal { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredInstallDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredInstallTime")]
        public string PreferredInstallTime { get; set; }
		public bool? HasExistingAgreements { get; set; }
    }
}