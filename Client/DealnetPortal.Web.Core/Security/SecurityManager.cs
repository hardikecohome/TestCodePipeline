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

        private const string EmptyUser = "Admin";//use administrator here because for testing empty username and password are using

        private const string CookieName = "DEALNET_AUTH_COOKIE";

        public SecurityManager(ISecurityServiceAgent securityService, IUserManagementServiceAgent userManagementService)
        {
            _securityService = securityService;
            _userManagementService = userManagementService;
        }

        public async Task<bool> Login(string userName, string password)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (password == null)
                throw new ArgumentNullException("password");

            if (string.IsNullOrWhiteSpace(userName))
            {
                userName = EmptyUser;
                password = EmptyUser;
            }

            var principal = await _securityService.Authenicate(userName, password);

            if (principal != null)
            {
                try
                {
                    SetUser(principal);
                    _securityService.SetAuthorizationHeader(principal);
                    return true;
                }
                catch (Exception ex)
                {
                    // log error
                    return false;
                }
            }
            return false;
        }

        public IPrincipal GetUser()
        {
            //HttpContext.Current.User
            var authCookie = HttpContext.Current.Request.Cookies[CookieName];
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
            }
        }

        public void Logout()
        {
            var httpCookie = HttpContext.Current?.Response.Cookies[CookieName];
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
                  DateTime.Now.Add(FormsAuthentication.Timeout), // ??
                  isPersistent,
                  identity.Token,
                  FormsAuthentication.FormsCookiePath);
                
                // Encrypt the ticket.
                var encTicket = FormsAuthentication.Encrypt(ticket);
                // Create the cookie.
                var authCookie = new HttpCookie(CookieName)
                {
                    Value = encTicket,
                    HttpOnly = true,
                    //Secure = true, Uncomment after enabling https
                    Expires = DateTime.Now.Add(FormsAuthentication.Timeout) //?
                };
                HttpContext.Current?.Response?.Cookies?.Set(authCookie); // ??
            }

        }
    }
}
