using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Web;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Web.Common.Security;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Common
{
    public class ApiBase
    {
        protected readonly string _uri;
        protected readonly string _fullUri;
        protected readonly IAuthenticationManager _authenticationManager;

        protected const string CultureCookieName = "DEALNET_CULTURE_COOKIE";

        public ApiBase(IHttpApiClient client, string controllerName)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;

            _authenticationManager = null;
            _uri = controllerName;
            _fullUri = string.Format("{0}/{1}", Client.Client.BaseAddress, _uri);
        }

        public ApiBase(IHttpApiClient client, string controllerName, IAuthenticationManager authenticationManager)
            : this(client, controllerName)
        {                        
            _authenticationManager = authenticationManager;
        }

        /// <summary>
        /// Http client
        /// </summary>
        protected IHttpApiClient Client { get; private set; }

        protected AuthenticationHeaderValue AuthenticationHeader
        {
            get
            {
                if (_authenticationManager?.User != null)
                {
                    var token = (_authenticationManager?.User?.Identity as UserIdentity)?.Token ?? (_authenticationManager?.User?.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(c => c.Type == "access_token")?.Value;
                    //return Authorization header
                    return new AuthenticationHeaderValue("Bearer", token ?? string.Empty);
                }
                return null;
            }
        }

        protected string CurrentCulture
        {
            get
            {
                var culture = HttpContext.Current?.Request.Cookies[CultureCookieName]?.Value ??
                              (HttpContext.Current?.Request.RequestContext.RouteData.Values["culture"] as string);
                return culture;
            }
        }
    }
}
