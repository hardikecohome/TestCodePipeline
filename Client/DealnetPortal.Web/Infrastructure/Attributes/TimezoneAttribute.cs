using System.Web.Mvc;
using DealnetPortal.Web.Common.Helpers;

namespace DealnetPortal.Web.Infrastructure.Attributes
{
    public class TimezoneAttribute : ActionFilterAttribute
    {
        private const string CookieName = "timezoneoffset";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Request.Cookies[CookieName] != null)
            {
                var timeOffSet = filterContext.HttpContext.Request.Cookies[CookieName].Value;
                int offset;

                var isValid = int.TryParse(timeOffSet, out offset);

                if (isValid)
                {
                    TimeZoneHelper.SetOffset(offset);

                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}