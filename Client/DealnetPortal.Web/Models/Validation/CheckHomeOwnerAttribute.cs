using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DealnetPortal.Web.Models.Validation
{
    public class CheckHomeOwnerAttribute : ValidationAttribute, IClientValidatable
    {
        private readonly string _addApplicantsProperty;

        public CheckHomeOwnerAttribute(string addApplicantsProperty)
        {
            _addApplicantsProperty = addApplicantsProperty;
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
            var homeOwner = (ApplicantPersonalInfo)value;
            
            if (homeOwner.IsHomeOwner || (addAplicants?.Any(ap => ap.IsHomeOwner) ?? false))
            {
                return ValidationResult.Success;
            }
            string errorMessage = GetErrorMessage(validationContext.ObjectType, validationContext.DisplayName);
            return new ValidationResult(errorMessage);
        }

        protected string GetErrorMessage(Type containerType, string displayName)
        {
            const string DefaultErrorMessage =
                "At lest one of the applicants should be home owner";
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
                ValidationType = "checkhomeowner",
            };
            rule.ValidationParameters.Add("other", _addApplicantsProperty);
            yield return rule;
        }
    }
}