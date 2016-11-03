using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Core.Security
{
    public class SecurityManager : ISecurityManager
    {
        private readonly ISecurityServiceAgent _securityService;
        private readonly IUserManagementServiceAgent _userManagementService;
        private readonly ILoggingService _loggingService;

        private const string EmptyUser = "Admin";//use administrator here because for testing empty username and password are using

        private readonly string _cookieName;

        public SecurityManager(ISecurityServiceAgent securityService, IUserManagementServiceAgent userManagementService,
            ILoggingService loggingService, PortalType portalType)
        {
            _securityService = securityService;
            _userManagementService = userManagementService;
            _loggingService = loggingService;
            _cookieName = "DEALNET_AUTH_COOKIE_" + portalType.ToString().ToUpper();
        }

        public async Task<IList<Alert>> Login(string userName, string password, string portalId)
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
                    SetUser(result.Item1);
                    _securityService.SetAuthorizationHeader(result.Item1);
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

        public IPrincipal GetUser()
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

        public void SetUser(IPrincipal user)
        {
            CreateCookie(user);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = user; // ?
            }
        }

        public void SetUserFromContext()
        {
            var user = GetUser();
            if (user != null)
            {
                SetUser(user);
                if (!_securityService.IsAutorizated())
                {
                    _securityService.SetAuthorizationHeader(user);
                }
            }
        }

        public void Logout()
        {
            var httpCookie = HttpContext.Current?.Response.Cookies[_cookieName];
            if (httpCookie != null)
            {
                httpCookie.Value = string.Empty;
            }

            if (HttpContext.Current != null)
            {
                HttpContext.Current.User = null;
            }
            _securityService.SetAuthorizationHeader(null);

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
                HttpContext.Current?.Response?.Cookies?.Set(authCookie); // ??
            }

        }
    }
}
