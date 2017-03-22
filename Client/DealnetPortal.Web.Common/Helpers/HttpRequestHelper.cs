using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class HttpRequestHelper
    {
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
    }
}
