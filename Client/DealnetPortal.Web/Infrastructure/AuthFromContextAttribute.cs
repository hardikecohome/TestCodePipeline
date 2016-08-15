﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using DealnetPortal.Web.Common.Security;

namespace DealnetPortal.Web.Infrastructure
{
    public class AuthFromContextAttribute : FilterAttribute, IAuthenticationFilter
    {
        private readonly ISecurityManager _securityManager = DependencyResolver.Current.GetService<ISecurityManager>();
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            _securityManager.SetUserFromContext();
        }

        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext) { }
    }
}
