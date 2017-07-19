using System;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Web.Http;
using DealnetPortal.Api.Core.Constants;
using DealnetPortal.Utilities.Logging;
using Microsoft.Owin.Security.OAuth;

namespace DealnetPortal.Api
{
    public static class WebApiConfig
    {
        private static readonly ILoggingService _loggingService = (ILoggingService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));

        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));            

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Formatters.Add(new BsonMediaTypeFormatter());
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;
        }

        public static void CheckConfigKeys()
        {
            Type type = typeof(WebConfigKeys);
            foreach (var key in type.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public))
            {
                var keyName = key.GetValue(null).ToString();
                if (ConfigurationManager.AppSettings[keyName] == null)
                {
                    _loggingService.LogError($"{keyName} KEY DON'T EXIST IN WEB CONFIG.");
                }
            }
        }
    }
}
