using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            bool httpsOn;
            if (!bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["HttpsOnProduction"], out httpsOn))
            {
                httpsOn = false;
            }
            if (httpsOn)
            {
                filters.Add(new RequreSecureConnectionFilter());
            }
            //filters.Add(new HandleErrorAttribute());
            //filters.Add(new AuthFromContextAttribute());
        }
    }
}
