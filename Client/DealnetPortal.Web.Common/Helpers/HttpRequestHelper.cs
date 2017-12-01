using System.IO;
using System.Web;
using System.Web.Routing;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class HttpRequestHelper
    {
        private const string CookieName = "timezoneoffset";

        public static RouteValueDictionary GetUrlReferrerRouteDataValues()
        {
            var fullUrl = HttpContext.Current.Request.UrlReferrer?.ToString();
            if (fullUrl == null) { return null; }
            var questionMarkIndex = fullUrl.IndexOf('?');
            string queryString = null;
            var url = fullUrl;
            if (questionMarkIndex != -1)
            {
                url = fullUrl.Substring(0, questionMarkIndex);
                queryString = fullUrl.Substring(questionMarkIndex + 1);
            }

            var request = new HttpRequest(null, url, queryString);
            var response = new HttpResponse(new StringWriter());
            var httpContext = new HttpContext(request, response);

            var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(httpContext));
            return routeData?.Values;
        }

        public static void TryGetTimezoneOffsetCookie()
        {
            if (HttpContext.Current.Request.Cookies[CookieName] == null) return;

            var timeOffSet = HttpContext.Current.Request.Cookies[CookieName].Value;
            int offset;
            var isValid = int.TryParse(timeOffSet, out offset);

            if (!isValid) return; 

            TimeZoneHelper.SetOffset(offset);
        }
    }
}
