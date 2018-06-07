using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;

using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models.Validation
{
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
            if(property == null)
            {
                return new ValidationResult(string.Format(
                    CultureInfo.CurrentCulture,
                    "Unknown property {0}",
                    new[] { _costProperty }
                ));
            }

            var cost = (decimal?)type.GetProperty(_costProperty).GetValue(instance, null);
            //var cost = (decimal?)property.GetValue(validationContext.ObjectInstance, null);
            var eqList = (List<NewEquipmentInformation>)value;

            if(eqList != null && eqList.Any())
            {
                if(eqList.Sum(e => e.Cost) > (double?)cost)
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
                ValidationType = "checkcost"
            };
            yield return rule;
        }
    }
}