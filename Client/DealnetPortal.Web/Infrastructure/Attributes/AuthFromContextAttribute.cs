using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Routing;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;

namespace DealnetPortal.Web.Infrastructure.Attributes
{
    public class AuthFromContextAttribute : FilterAttribute, IAuthenticationFilter
    {
        private readonly ISecurityManager _securityManager = DependencyResolver.Current.GetService<ISecurityManager>();
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            //_securityManager.SetUserFromContext();
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.HttpContext.User == null || !filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                if (filterContext.HttpContext.Request.RawUrl.Contains("LogOff"))
                {
                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" }));
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login", returnUrl = filterContext.HttpContext.Request.RawUrl }));
                }
            }
                
        }
    }
}
