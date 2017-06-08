using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace DealnetPortal.Web.Infrastructure
{
    public class ClaimsProvider
    {
        public static IList<Claim> GetClaimsFromRoles(string[] roles)
        {
            var claims = new List<Claim>();
            return claims;
        }
    }
}