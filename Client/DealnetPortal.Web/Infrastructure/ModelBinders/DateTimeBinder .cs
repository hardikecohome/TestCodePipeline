using System;
using System.Globalization;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Helpers;

namespace DealnetPortal.Web.Infrastructure.ModelBinders
{
    public class DateTimeBinder : IModelBinder
    {
        private static readonly CultureInfo ENDateTimeCulture = new CultureInfo("en");
        private static readonly CultureInfo FRDateTimeCulture = new CultureInfo("fr");
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            DateTime dateTime;

            var isDate = DateTime.TryParse(value.AttemptedValue, ENDateTimeCulture , DateTimeStyles.None, out dateTime) ||
                DateTime.TryParse(value.AttemptedValue, FRDateTimeCulture, DateTimeStyles.None, out dateTime);

            if (!isDate)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.Resources.TheDateMustBeInCorrectFormat);
            }

            return dateTime;
        }
    }

    public class NullableDateTimeBinder : IModelBinder
    {
        private static readonly CultureInfo ENDateTimeCulture = new CultureInfo("en");
        private static readonly CultureInfo FRDateTimeCulture = new CultureInfo("fr");
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            if (string.IsNullOrWhiteSpace(value.AttemptedValue))
            {
                return null;
            }

            DateTime dateTime;

            var isDate = DateTime.TryParse(value.AttemptedValue, ENDateTimeCulture, DateTimeStyles.None, out dateTime) ||
                            DateTime.TryParse(value.AttemptedValue, FRDateTimeCulture, DateTimeStyles.None, out dateTime);

            if (!isDate)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.Resources.TheDateMustBeInCorrectFormat);
                return null;
            }

            return dateTime;
        }
    }
}
