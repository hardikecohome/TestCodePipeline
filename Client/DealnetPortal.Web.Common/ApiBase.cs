using System;
using DealnetPortal.Api.Core.ApiClient;

namespace DealnetPortal.Web.Common
{
    public class ApiBase
    {
        protected readonly string _uri;
        protected readonly string _fullUri;

        public ApiBase(IHttpApiClient client, string controllerName)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;
            
            _uri = controllerName;
            _fullUri = string.Format("{0}/{1}", Client.Client.BaseAddress, _uri);
        }

        /// <summary>
        /// Http client
        /// </summary>
        protected IHttpApiClient Client { get; private set; }
    }
}
