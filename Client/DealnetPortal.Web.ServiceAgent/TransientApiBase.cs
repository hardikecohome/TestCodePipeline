using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;

namespace DealnetPortal.Web.ServiceAgent
{
    public class TransientApiBase
    {
        protected readonly string _uri;
        protected readonly string _fullUri;
        private readonly ITransientHttpApiClient _client;

        public TransientApiBase(ITransientHttpApiClient client, string controllerName)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _client = client;

            _uri = controllerName;
            _fullUri = $"{client.BaseAddress}/{_uri}";
        }

        /// <summary>
        /// Http client
        /// </summary>
        protected IHttpApiClient Client => _client.Client;
    }
}
