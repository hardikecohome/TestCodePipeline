using System;
using System.Net.Http.Headers;
using DealnetPortal.Api.Core.ApiClient;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.Common
{
    public class ApiBase
    {
        protected readonly string _uri;
        protected readonly string _fullUri;
        protected readonly IAuthenticationManager _authenticationManager;

        public ApiBase(IHttpApiClient client, string controllerName, IAuthenticationManager authenticationManager)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;

            _authenticationManager = authenticationManager;
            _uri = controllerName;
            _fullUri = string.Format("{0}/{1}", Client.Client.BaseAddress, _uri);
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
                    
                }
                return null;
            }
        }
    }
}
