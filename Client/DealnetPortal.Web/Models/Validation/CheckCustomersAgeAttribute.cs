using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DealnetPortal.Web.Models.Validation
{
    public class CheckCustomersAgeAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _addApplicantsProperty;
        private readonly int _maxAge;

        public CheckCustomersAgeAttribute(string addApplicantsProperty, int maxAge)
        {
            _addApplicantsProperty = addApplicantsProperty;
            _maxAge = maxAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            object instance = validationContext.ObjectInstance;
            Type type = validationContext.ObjectType;

            var property = validationContext.ObjectType.GetProperty(_addApplicantsProperty);
            if (property == null)
            {
                return new ValidationResult(string.Format(
                    CultureInfo.CurrentCulture,
                    "Unknown property {0}",
                    new[] { _addApplicantsProperty }
                ));
            }

            var addAplicants = (IList<ApplicantPersonalInfo>)type.GetProperty(_addApplicantsProperty).GetValue(instance, null);
            //var cost = (decimal?)property.GetValue(validationContext.ObjectInstance, null);
            //var eqList = (List<NewEquipmentInformation>)value;        
            var homeOwner = (ApplicantPersonalInfo) value;

            var isUnderAge = new Func<DateTime, bool>(date => (DateTime.Now - date) < (DateTime.Now.AddYears(_maxAge) - DateTime.Now));

            if (homeOwner.BirthDate.HasValue && isUnderAge(homeOwner.BirthDate.Value) ||
                (addAplicants?.Any(ap => ap.BirthDate.HasValue && isUnderAge(ap.BirthDate.Value)) ?? false))
            {
                return ValidationResult.Success;
            }
            string errorMessage = GetErrorMessage(validationContext.ObjectType, validationContext.DisplayName);
            return new ValidationResult(errorMessage);
        }

        protected string GetErrorMessage(Type containerType, string displayName)
        {
            const string DefaultErrorMessage =
                "At least one of applicants should be under 75 age";
            ModelMetadata metadata = ModelMetadataProviders.Current.GetMetadataForProperty(null, containerType,
                                                                                           _addApplicantsProperty);
            var otherDisplayName = metadata.GetDisplayName();
            return ErrorMessage ?? DefaultErrorMessage;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.GetDisplayName()),
                ValidationType = "checkcustomersage",
            };
            rule.ValidationParameters.Add("other", _addApplicantsProperty);
            yield return rule;
        }
    }
}