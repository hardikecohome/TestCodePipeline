﻿using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DealnetPortal.Api.App_Start;
using DealnetPortal.Api.Core.Helpers;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api
{
    public class WebApiApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            UnityConfig.RegisterComponents();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            DatabaseConfig.Initialize();
            AutoMapperConfig.Configure();
            WebApiConfig.CheckConfigKeys();

            //GlobalConfiguration.appConfiguration.Services.Replace(typeof(IHttpControllerSelector),
            //    new LocalizedControllerSelector(GlobalConfiguration.appConfiguration));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var userLanguage = ((HttpApplication) sender)?.Context?.Request?.UserLanguages?[0];
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureHelper.FilterCulture(userLanguage));
        }

        protected void Application_Error(object sender, EventArgs e)
        {            
            Exception exception = Server.GetLastError();
            Server.ClearError();

            var loggingService = (ILoggingService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));
            loggingService?.LogError("Server error:", exception);
        }
    }
}
