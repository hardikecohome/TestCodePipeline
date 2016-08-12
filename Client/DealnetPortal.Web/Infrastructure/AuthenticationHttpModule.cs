using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Common.Security;

namespace DealnetPortal.Web.Infrastructure
{
    public class AuthenticationHttpModule : IHttpModule
    {
        private readonly ISecurityManager _securityManager = DependencyResolver.Current.GetService<ISecurityManager>();
        public void Init(HttpApplication context)
        {
            context.PostAuthenticateRequest += Authenticate;
        }

        private void Authenticate(Object source, EventArgs e)
        {
            _securityManager.SetUserFromContext();
        }

        public void Dispose() { }
    }
}
