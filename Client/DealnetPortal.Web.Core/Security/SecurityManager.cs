using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Web.Common.Security;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Core.Security
{
    public class SecurityManager : ISecurityManager
    {
        public SecurityManager()
        {
            //_authenticationManager = authenticationManager;
        }

        public bool Login(string userName, string password)
        {

            try
            {            
                //HttpContext.Current.Response.Cookies.Add();
                //stub for a service

                var claims = new List<Claim>();

                var userId = Guid.NewGuid().ToString();
                //var userLoginName = "user1";

                // create required claims
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId));
                claims.Add(new Claim(ClaimTypes.Name, userName));

                // custom – my serialized AppUserState object
                claims.Add(new Claim("userState", userName));

                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                //_authenticationManager.SignIn(new AuthenticationProperties()
                //{
                //    AllowRefresh = true,
                //    IsPersistent = false,
                //    ExpiresUtc = DateTime.UtcNow.AddDays(7)
                //}, identity);


                //_authenticationManager.SignIn();

                return true;
            }
            catch (Exception)
            {
                // log error
                return false;
            }
        }

        public IPrincipal GetUser()
        {
            throw new NotImplementedException();
        }

        public void SetUser(IPrincipal user)
        {
            throw new NotImplementedException();
        }

        public void Logout()
        {
            //_authenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie,
            //                        DefaultAuthenticationTypes.ExternalCookie);

        }
    }
}
