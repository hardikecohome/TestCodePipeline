using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DealnetPortal.Web.Common.Helpers;

namespace DealnetPortal.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

           // routes.MapRoute(
           //    name: "",
           //    url: "NewApplication/{action}/{contractId}",
           //    defaults: new { controller = "NewRental", contractId = UrlParameter.Optional }
           //);

           // routes.MapRoute(
           //     name: "Default",
           //     url: "{controller}/{action}/{id}",
           //     defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
           // );

            routes.MapRoute(
                name: "LocalizedNewApplication",
                url: "{lang}/NewApplication/{action}/{id}",
                constraints: new { lang = @"(\w{2})|(\w{2}-\w{2})" },   // en or en-US
                defaults: new { lang = CultureHelper.GetDefaultCulture(), controller = "NewRental", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DefaultLocalized",
                url: "{lang}/{controller}/{action}/{id}",
                constraints: new { lang = @"(\w{2})|(\w{2}-\w{2})" },   // en or en-US
                defaults: new { lang = CultureHelper.GetDefaultCulture(), controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

        }
    }
}
