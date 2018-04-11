using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Models.Enumeration;

using ContractState = DealnetPortal.Api.Common.Enumeration.ContractState;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class EquipmentInformationViewModelNew
    {
        public EquipmentInformationViewModelNew()
        {
            SalesRepInformation = new SalesRepInformation();
            Conditions = new ContractConditions();
        }

        [Display(ResourceType = typeof(Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }
        public List<InstallationPackageInformation> InstallationPackages { get; set; }
        [Display(ResourceType = typeof(Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public List<string> CustomerComments { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredInstallDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? PrefferedInstallDate { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredInstallTime")]
        public string PrefferedInstallTime { get; set; }

        public SalesRepInformation SalesRepInformation { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }
        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TotalMonthlyPaymentIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "TotalMonthlyPayment")]
        public double? TotalMonthlyPayment { get; set; }

        [CustomRequired]
        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestedTermMustBe3Max")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "RequestedTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "RequestedTerm")]
        public int RequestedTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LoanTermMustBe3Max")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LoanTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "LoanTerm")]
        public int? LoanTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AmortizationTermMustBe3Max")]
        [RegularExpression(@"^[1-9]\d{0,2}$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AmortizationTermIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AmortizationTerm")]
        public int? AmortizationTerm { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "DeferralType")]
        public LoanDeferralType LoanDeferralType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "DeferralType")]
        public RentalDeferralType RentalDeferralType { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "CustomerRateIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomerRatePercentage")]
        public double? CustomerRate { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "YourRateIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "YourRate")]
        public double? DealerCost { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AdminFeeIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AdminFee")]
        public double? AdminFee { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "DownPaymentIncorrectFormat")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "DownPayment")]
        public double? DownPayment { get; set; }

        public int? ContractId { get; set; }

        public ContractState ContractState { get; set; }

        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }

        public decimal? CreditAmount { get; set; }

        public double? ValueOfDeal { get; set; }

        public int? SelectedRateCardId { get; set; }

        public int? CustomerRiskGroupId { get; set; }

        public string DealProvince { get; set; }

        public TierViewModel DealerTier { get; set; }

        public ContractConditions Conditions { get; set; }
    }

    public class ContractConditions
    {
        [CustomRequired]
        public bool? IsAdminFeePaidByCustomer { get; set; }

        public bool? RateCardValid { get; set; }

        public string DealProvince { get; set; }

        public bool IsClarityDealer { get; set; } = false;

        public bool IsOldClarityDeal { get; set; } = false;

        public bool? IsClarityProgram { get; set; }

        public bool IsCustomRateCardSelected { get; set; }

        public bool IsBeaconUpdated { get; set; }

        public bool IsCustomerFoundInCreditBureau { get; set; }

        public bool IsSubmittedWithoutCustomerRateCard { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        public bool IsFirstStepAvailable { get; set; }

        public bool IsNewContract { get; set; }
        public bool FullUpdate { get; set; }
    }

    public class SalesRepInformation
    {
        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        public bool IniatedContract { get; set; }
        public bool NegotiatedAgreement { get; set; }
        public bool ConcludedAgreement { get; set; }
    }
}