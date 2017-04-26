using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Models.Enumeration;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class OwinSecurityManager : SecurityManager, ISecurityManager
    {
        public OwinSecurityManager(ISecurityServiceAgent securityService,
            IUserManagementServiceAgent userManagementService,
            ILoggingService loggingService, PortalType portalType)
            : base(securityService, userManagementService, loggingService, portalType)
        {
        }

        public override IPrincipal GetUser()
        {
            var user = AuthenticationManager.User;            
            return user;
        }

        public override void SetUser(IPrincipal user)
        {
            AuthenticationProperties properties = null;
            var dict = new Dictionary<string, string>();

            UserIdentity uidentity = user.Identity as UserIdentity;            
            
            if (uidentity != null)
            {
                dict.Add("Token", uidentity.Token);
                properties = new AuthenticationProperties(dict)
                {
                    IsPersistent = true
                };
            }

            if (user.Identity.IsAuthenticated)
            {
                if (properties != null)
                {
                    AuthenticationManager.SignIn(properties, user.Identity as ClaimsIdentity);
                }
                else
                {
                    AuthenticationManager.SignIn(user.Identity as ClaimsIdentity);
                }
            }           
        }

        public override void Logout()
        {            
            _securityService.SetAuthorizationHeader(null);            
            _userManagementService?.Logout();
            AuthenticationManager?.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        private IAuthenticationManager AuthenticationManager => HttpContext.Current?.Request.GetOwinContext().Authentication;
    }
}