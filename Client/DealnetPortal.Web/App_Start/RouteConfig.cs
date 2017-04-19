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

            routes.MapRoute(
               name: "",
               url: "CustomerForm/{action}/{contractId}/{culture}/{hashDealerName}",
               defaults: new { controller = "CustomerForm", action = "Index" }
           );

            routes.MapRoute(
               name: "",
               url: "CustomerForm/{culture}/{hashDealerName}",
               defaults: new { controller = "CustomerForm", action = "Index" }
           );

            routes.MapRoute(
               name: "",
               url: "NewApplication/{action}/{contractId}",
               defaults: new { controller = "NewRental", contractId = UrlParameter.Optional }
           );

            routes.MapRoute(
               name: "",
               url: "Settings/{action}/{hashDealerName}",
               defaults: new { controller = "Settings" }
           );

            routes.MapRoute(
               name: "",
               url: "Settings/{Favicon}/{hashDealerName}/{version}",
               defaults: new { controller = "Settings", action = "Favicon" }
           );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            
        }
    }
}
