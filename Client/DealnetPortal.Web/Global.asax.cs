﻿using System;
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
using DealnetPortal.Web.Common.Culture;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Attributes;
using DealnetPortal.Web.Infrastructure.ModelBinders;

namespace DealnetPortal.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            GlobalFilters.Filters.Add(new TimezoneAttribute());
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfig.Configure();
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Name;
            MvcHandler.DisableMvcResponseHeader = true;
            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new NullableDateTimeBinder());
            ClientDataTypeModelValidatorProvider.ResourceClassKey = "Messages";
            DefaultModelBinder.ResourceClassKey = "Messages";
            DataAnnotationsModelValidatorProvider.RegisterAdapter(
                typeof(CustomRequiredAttribute),
                typeof(RequiredAttributeAdapter));

            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory(new LocalizedControllerActivator(DependencyResolver.Current.GetService<ICultureManager>())));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var app = sender as HttpApplication;
            if (app != null)
            {
                app.Context?.Response.Headers.Remove("Server");
            }
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
