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

            var addAplicants = (IList<ApplicantPersonalInfo>)type.GetProperty(_addApplicantsProperty)?.GetValue(instance, null);
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
            return ErrorMessage ?? Resources.Resources.AtLeastOneHomeOwner;
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