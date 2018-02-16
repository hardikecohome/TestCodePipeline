using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.AspNet.Identity;

[assembly: OwinStartup(typeof(DealnetPortal.Web.Startup))]

namespace DealnetPortal.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {
            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            CookieAuthenticationProvider provider = new CookieAuthenticationProvider();

            var originalHandler = provider.OnApplyRedirect;

            //Our logic to dynamically modify the path (maybe needs some fine tuning)
            provider.OnApplyRedirect = context =>
            {
                var mvcContext = new HttpContextWrapper(HttpContext.Current);
                var routeData = RouteTable.Routes.GetRouteData(mvcContext);

                //Get the current language  
                RouteValueDictionary routeValues = new RouteValueDictionary();
                routeValues.Add("culture", routeData.Values["culture"]);

                //Reuse the RetrunUrl
                Uri uri = new Uri(context.RedirectUri);
                string returnUrl = HttpUtility.ParseQueryString(uri.Query)[context.Options.ReturnUrlParameter];
                routeValues.Add(context.Options.ReturnUrlParameter, returnUrl);

                //Overwrite the redirection uri
                context.RedirectUri = url.Action("Login", "Account", routeValues);
                originalHandler.Invoke(context);
            };
            provider.OnValidateIdentity = ctx =>
            {
                //MyClaimsIdentityObject si = MyApp.Identity.Current();
                //if (si == null || si.UserId == 0 || si.CustomerId == 0)
                {
                    //ctx.RejectIdentity();
                    // what needs to happen here for a return value?
                }
                return Task.FromResult<int>(0);
            };
            //provider.OnValidateIdentity = MySecurityStampValidator.OnValidateIdentity(
            //    validateInterval: TimeSpan.FromMinutes(0),
            //    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager));

            //provider.OnResponseSignIn = (context) =>
            //{
            //    context.Properties.IsPersistent = false;
            //    context.Properties.ExpiresUtc = DateTimeOffset.Now.AddSeconds(10);
            //};

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString(url.Action("Login", "Account")),
                Provider = provider,
            });
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

        }
    }
}