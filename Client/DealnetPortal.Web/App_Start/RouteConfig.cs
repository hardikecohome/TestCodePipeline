using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Extensions;

namespace DealnetPortal.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "CustomerFormWithCulture",
                url: "{culture}/CustomerForm/{hashDealerName}",
                defaults: new { controller = "CustomerForm", action = "Index" },
                constraints: new { culture = @"en|fr" }
            );
            routes.MapRoute(
               name: "CustomerFormDefault",
               url: "CustomerForm/{hashDealerName}",
               defaults: new {controller = "CustomerForm", action = "Index" }
           );
           routes.MapRoute(
               name: "CustomerFormActionWithCulture",
               url: "{culture}/CustomerForm/{action}/{contractId}/{hashDealerName}",
               defaults: new { controller = "CustomerForm", action = "Index" },
               constraints: new { culture = @"en|fr" }
           );
            routes.MapRoute(
               name: "CustomerFormActionDefault",
               url: "CustomerForm/{action}/{contractId}/{hashDealerName}",
               defaults: new { controller = "CustomerForm", action = "Index" }
           );
            routes.MapRoute(
                name: "OnboardingWithCulture",
                url: "{culture}/Dealer/{action}/{key}",
                defaults: new { controller = "Dealer" },
                constraints: new { culture = @"en|fr" }
            );
            routes.MapRoute(
                name: "OnboardingDefault",
                url: "Dealer/{action}/{key}",
                defaults: new {  controller = "Dealer"}
            );

            routes.MapRoute(
              name: "NewApplicationWithCulture",
              url: "{culture}/NewApplication/{action}/{contractId}",
              defaults: new { controller = "NewRental", contractId = UrlParameter.Optional },
              constraints: new { culture = @"en|fr" }
          );

            routes.MapRoute(
               name: "NewApplicationDefault",
               url: "NewApplication/{action}/{contractId}",
               defaults: new { controller = "NewRental", contractId = UrlParameter.Optional }
           );
           

            routes.MapRoute(
               name: "",
               url: "Settings/{action}/{hashDealerName}",
               defaults: new {  controller = "Settings" }
           );
        
            routes.MapRoute(
               name: "",
               url: "Settings/{Favicon}/{hashDealerName}/{version}",
               defaults: new { controller = "Settings", action = "Favicon" }
           );

            routes.MapRoute(
                name: "DefaultWithCulture",
                url: "{culture}/{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                constraints: new { culture = @"en|fr"}
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new {  controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
