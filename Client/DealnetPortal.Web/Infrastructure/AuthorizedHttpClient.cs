using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Web.Common.Security;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Infrastructure
{
    public class AuthorizedHttpClient : HttpApiClient, IHttpApiClient
    {
        private readonly IAuthenticationManager _authenticationManager;
        public AuthorizedHttpClient(string baseAddress, IAuthenticationManager authenticationManager) : base(baseAddress)
        {
            _authenticationManager = authenticationManager;
            SetAuthorizationHeader(_authenticationManager.User);
        }

        private void SetAuthorizationHeader(IPrincipal principal)
        {
            //var token = (principal?.Identity as UserIdentity)?.Token ?? (principal?.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;

            ////Set Authorization header
            //Client.DefaultRequestHeaders.Authorization =
            //   new AuthenticationHeaderValue("Bearer", token ?? string.Empty);
        }
    }
}