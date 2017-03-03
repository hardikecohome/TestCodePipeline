using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DealnetPortal.Web.App_Start;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Culture;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.ModelBinders;

namespace DealnetPortal.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.Configure();
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new NullableDateTimeBinder());

            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new LocalizedControllerActivator(DependencyResolver.Current.GetService<ICultureManager>())));
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            if (Debugger.IsAttached) { return; }
            Exception exception = Server.GetLastError();
            Server.ClearError();

            Response.RedirectToRoute(new RouteValueDictionary(new { controller = "Info", action = "Error" }));
        }
    }
}
