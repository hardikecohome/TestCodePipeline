using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.Enumeration;
using AgreementType = DealnetPortal.Web.Models.Enumeration.AgreementType;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class ContactAndPaymentInfoViewModelNew
    {
        public int? ContractId { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }
    }

    public class EquipmentInformationViewModelNew
    {
        [Display(ResourceType = typeof(Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "HouseSizeSquareFeet")]
        public double? HouseSize { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "CustomersComment")]
        public List<string> CustomerComments { get; set; }
        
        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

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

        public bool FullUpdate { get; set; }

        public int? ContractId { get; set; }

        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        public bool IsNewContract { get; set; }

        public decimal? CreditAmount { get; set; }

        public double? ValueOfDeal { get; set; }

        public int? SelectedRateCardId { get; set; }

        public TierDTO DealerTier { get; set; }
    }

    public class EquipmentInformationViewModel
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "TypeOfAgreement")]
        public AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}([.,][0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "TotalMonthlyPaymentIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "TotalMonthlyPayment")]
        public double? TotalMonthlyPayment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "RequestedTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "RequestedTerm")]
        public int? RequestedTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "LoanTermMustBe3Max")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "LoanTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanTerm")]
        public int? LoanTerm { get; set; }

        [Range(0, 999, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "AmortizationTermMustBe3Max")]
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AmortizationTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AmortizationTerm")]
        public int? AmortizationTerm { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "DeferralType")]
        public LoanDeferralType LoanDeferralType { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "DeferralType")]
        public RentalDeferralType RentalDeferralType { get; set; }
                          
        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CustomerRateIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "CustomerRatePercentage")]
        public double? CustomerRate { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "AdminFeeIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AdminFee")]
        public double? AdminFee { get; set; }

        [RegularExpression(@"(^[0]?|(^[1-9]\d{0,11}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "DownPaymentIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "DownPayment")]
        public double? DownPayment { get; set; }

        [CustomRequired]
        [StringLength(50, ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        [StringLength(500, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMaximum")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }

        public bool FullUpdate { get; set; }

        public int? ContractId { get; set; }

        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        public decimal? CreditAmount { get; set; }

        public double? ValueOfDeal { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "EstimatedInstallationDate")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? EstimatedInstallationDate { get; set; }
    }

    public class CheckEquipmentsCostAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _costProperty;

        public CheckEquipmentsCostAttribute(string costProperty)
        {
            _costProperty = costProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            Type type = validationContext.ObjectType;

            var property = validationContext.ObjectType.GetProperty(_costProperty);
            if (property == null)
            {
                return new ValidationResult(string.Format(
                    CultureInfo.CurrentCulture,
                    "Unknown property {0}",
                    new[] { _costProperty }
                ));
            }

            var cost = (decimal?)type.GetProperty(_costProperty).GetValue(instance, null);
            //var cost = (decimal?)property.GetValue(validationContext.ObjectInstance, null);
            var eqList = (List<NewEquipmentInformation>) value;

            if (eqList != null && eqList.Any())
            {
                if (eqList.Sum(e => e.Cost) > (double?)cost)
                {
                    string errorMessage = GetErrorMessage(validationContext.ObjectType, validationContext.DisplayName);
                    return new ValidationResult(errorMessage);
                }
            }
            return ValidationResult.Success;
        }

        protected string GetErrorMessage(Type containerType, string displayName)
        {
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, containerType,
                                                                                           _costProperty);
            var otherDisplayName = metadata.GetDisplayName();
            return ErrorMessage ?? Resources.Resources.TotalCostGreaterThanAmount; //string.Format(DefaultErrorMessage, displayName, otherDisplayName);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = Resources.Resources.TotalCostGreaterThanAmount,
                ValidationType = "checkcost",
            };
            yield return rule;
        }
    }

    //public class DateComesLaterAttribute : ValidationAttribute
    //{
    //    public const string DefaultErrorMessage = "'{0}' must be after '{1}'";

    //    protected readonly string OtherDateProperty;

    //    public DateComesLaterAttribute(string otherDateProperty)
    //    {
    //        OtherDateProperty = otherDateProperty;
    //    }

    //    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    //    {
    //        object instance = validationContext.ObjectInstance;
    //        Type type = validationContext.ObjectType;

    //        var earlierDate = (DateTime?)type.GetProperty(OtherDateProperty).GetValue(instance, null);
    //        var date = (DateTime?)value;

    //        if (date > earlierDate)
    //            return ValidationResult.Success;

    //        string errorMessage = GetErrorMessage(validationContext.ObjectType, validationContext.DisplayName);

    //        return new ValidationResult(errorMessage);
    //    }

    //    protected string GetErrorMessage(Type containerType, string displayName)
    //    {
    //        ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, containerType,
    //                                                                                       OtherDateProperty);
    //        var otherDisplayName = metadata.GetDisplayName();
    //        return ErrorMessage ?? string.Format(DefaultErrorMessage, displayName, otherDisplayName);
    //    }
    //}
}