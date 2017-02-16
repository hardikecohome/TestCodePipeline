using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class EquipmentInformationViewModel
    {
        [Display(ResourceType = typeof (Resources.Resources), Name = "TypeOfAgreement")]
        public Common.Enumeration.AgreementType AgreementType { get; set; }

        public List<NewEquipmentInformation> NewEquipment { get; set; }

        [RegularExpression(@"^[1-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "TotalMonthlyPaymentIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "TotalMonthlyPayment")]
        public double? TotalMonthlyPayment { get; set; }

        public List<ExistingEquipmentInformation> ExistingEquipment { get; set; }
        
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "RequestedTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "RequestedTerm")]
        public int? RequestedTerm { get; set; }
        
        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "LoanTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "LoanTerm")]
        public int? LoanTerm { get; set; }

        [RegularExpression(@"^[0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "AmortizationTermIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AmortizationTerm")]
        public int? AmortizationTerm { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "DeferralType")]
        public Common.Enumeration.LoanDeferralType LoanDeferralType { get; set; }

        [Display(ResourceType = typeof (Resources.Resources), Name = "DeferralType")]
        public Common.Enumeration.RentalDeferralType RentalDeferralType { get; set; }

	    [RegularExpression(@"(^[0]?|(^[1-9]\d{0,1}))([.,][0-9]{1,2})?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "CustomerRateIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "CustomerRatePercentage")]
        public double? CustomerRate { get; set; }

        [RegularExpression(@"^[0-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "AdminFeeIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "AdminFee")]
        public double? AdminFee { get; set; }

        [RegularExpression(@"^[0-9]\d{0,11}(\.[0-9][0-9]?)?$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "DownPaymentIncorrectFormat")]
        [Display(ResourceType = typeof (Resources.Resources), Name = "DownPayment")]
        public double? DownPayment { get; set; }

        [Required]
        [StringLength(50)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "SalesRep")]
        [RegularExpression(@"^[^0-9]+$", ErrorMessageResourceType = typeof (Resources.Resources), ErrorMessageResourceName = "SalesRepIncorrectFormat")]
        public string SalesRep { get; set; }

        [StringLength(500)]
        [Display(ResourceType = typeof (Resources.Resources), Name = "ContractNotes")]
        public string Notes { get; set; }

        public bool FullUpdate { get; set; }

        public int? ContractId { get; set; }

        public ProvinceTaxRateDTO ProvinceTaxRate { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }

        public decimal? CreditAmount { get; set; }

        public double? ValueOfDeal { get; set; }        
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