using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DealnetPortal.Api.App_Start;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Infrastucture;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api
{
    public class WebApiApplication : System.Web.HttpApplication
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

            //GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector),
            //    new LocalizedControllerSelector(GlobalConfiguration.Configuration));
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            var userLanguage = ((HttpApplication) sender)?.Context?.Request?.UserLanguages?[0];
            Thread.CurrentThread.CurrentCulture =
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureHelper.FilterCulture(userLanguage));
        }
    }
}
