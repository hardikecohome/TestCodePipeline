using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Helpers;
using DealnetPortal.Web.Common.Culture;

namespace DealnetPortal.Web.Infrastructure.ModelBinders
{
    public class DateTimeBinder : IModelBinder
    {
        private static readonly CultureInfo DefaultDateTimeCulture = new CultureInfo("en");
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            DateTime dateTime;
            var currentCulture = CultureHelper.GetCurrentCulture();

            var isDate = DateTime.TryParse(value.AttemptedValue, string.IsNullOrEmpty(currentCulture) ? DefaultDateTimeCulture : new CultureInfo(currentCulture), DateTimeStyles.None, out dateTime);

            if (!isDate)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.Resources.TheDateMustBeInCorrectFormat);
            }

            return dateTime;
        }
    }

    public class NullableDateTimeBinder : IModelBinder
    {
        private static readonly CultureInfo DefaultDateTimeCulture = new CultureInfo("en");
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            bindingContext.ModelState.SetModelValue(bindingContext.ModelName, value);

            if (string.IsNullOrWhiteSpace(value.AttemptedValue))
            {
                return null;
            }

            DateTime dateTime;
            var currentCulture = CultureHelper.GetCurrentCulture();

            var isDate = DateTime.TryParse(value.AttemptedValue,string.IsNullOrEmpty(currentCulture) ? DefaultDateTimeCulture : new CultureInfo(currentCulture), DateTimeStyles.None, out dateTime);

            if (!isDate)
            {
                bindingContext.ModelState.AddModelError(bindingContext.ModelName, Resources.Resources.TheDateMustBeInCorrectFormat);
                return null;
            }

            return dateTime;
        }
    }
}
