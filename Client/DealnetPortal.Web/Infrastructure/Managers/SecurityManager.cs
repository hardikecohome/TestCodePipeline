using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Models.Enumeration;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class SecurityManager : ISecurityManager
    {
        protected readonly ISecurityServiceAgent _securityService;
        protected readonly IUserManagementServiceAgent _userManagementService;
        protected readonly ILoggingService _loggingService;

        protected const string EmptyUser = "Admin";//use administrator here because for testing empty username and password are using

        protected readonly string _cookieName;

        public SecurityManager(ISecurityServiceAgent securityService, IUserManagementServiceAgent userManagementService,
            ILoggingService loggingService, PortalType portalType)
        {
            _securityService = securityService;
            _userManagementService = userManagementService;
            _loggingService = loggingService;
            _cookieName = FormsAuthentication.FormsCookieName;
                //"DEALNET_AUTH_COOKIE_" + portalType.ToString().ToUpper();
        }

        public virtual async Task<IList<Alert>> Login(string userName, string password, string portalId)
        {
            var alerts = new List<Alert>();

            if (userName == null)
                throw new ArgumentNullException("userName");
            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = EmptyUser;
                password = EmptyUser;
            }

            var result = await _securityService.Authenicate(userName, password, portalId);

            if (result?.Item1 != null)
            {
                try
                {
                    var claimsPrincipal = result.Item1 as ClaimsPrincipal;
                    if (claimsPrincipal != null)
                    {
                        var roles =
                            claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToArray();
                        var rolesClaims = ClaimsProvider.GetClaimsFromRoles(roles);
                        if (rolesClaims.Any())
                        {
                            (claimsPrincipal.Identity as ClaimsIdentity)?.AddClaims(rolesClaims);
                        }
                    }

                    SetUser(result.Item1);
                    //_securityService.SetAuthorizationHeader(result.Item1);
                }
                catch (Exception ex)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Message = ex.ToString()
                    });
                    _loggingService.LogError("Error on Login", ex);
                    return result.Item2;
                }
            }
            if (result?.Item2 != null && result.Item2.Any(x => x.Type == AlertType.Error))
            {
                alerts.AddRange(result.Item2);
            }
            return alerts;
        }

        public virtual IPrincipal GetUser()
        {
            //HttpContext.Current.User
            var authCookie = HttpContext.Current.Request.Cookies[_cookieName];
            if (!string.IsNullOrEmpty(authCookie?.Value))
            {
                var ticket = FormsAuthentication.Decrypt(authCookie.Value);
                if (ticket != null)
                {
                    var claims = new List<Claim>() { new Claim(ClaimTypes.Name, ticket.Name) };                    
                    var currentUser = new UserPrincipal(new UserIdentity(claims) {Token = ticket.UserData});
                    return currentUser;
                }
            }
            return null;
        }

        public virtual void SetUser(IPrincipal user)
        {
            CreateCookie(user);
            if (HttpContext.Current != null)
            {            
                HttpContext.Current.User = user; // ?
            }
        }        

        public virtual void Logout()
        {
            var httpCookie = HttpContext.Current?.Response.Cookies[_cookieName];
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
                //httpCookie.Expires = DateTime.Now.AddDays(-1d);
                //HttpContext.Current?.Response?.Cookies?.Set(httpCookie);
            }

            _securityService.SetAuthorizationHeader(null);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = null;
            }

            _userManagementService?.Logout();
        }

        private void CreateCookie(IPrincipal user, bool isPersistent = false)
        {
            UserIdentity identity = user.Identity as UserIdentity;

            if (identity != null)
            {
                var ticket = new FormsAuthenticationTicket(
                  1,
                  identity.Name,
                  DateTime.Now,
                  DateTime.Now.Add(TimeSpan.FromHours(5)),
                  isPersistent,
                  identity.Token,
                  FormsAuthentication.FormsCookiePath);
                
                // Encrypt the ticket.
                var encTicket = FormsAuthentication.Encrypt(ticket);
                // Create the cookie.
                var authCookie = new HttpCookie(_cookieName)
                {
                    Value = encTicket,
                    HttpOnly = true,
                    //Secure = true, Uncomment after enabling https
                    Expires = DateTime.Now.Add(TimeSpan.FromHours(5))
                };
                HttpContext.Current?.Response?.Cookies?.Add(authCookie); // ??
            }

        }
    }
}
