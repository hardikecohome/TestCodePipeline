using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Common.Api;

namespace DealnetPortal.Web.ServiceAgent
{
    public class ApiBase
    {
        protected readonly string _uri;

        public ApiBase(IHttpApiClient client, string controllerName)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            Client = client;

            _uri = controllerName;
        }

        /// <summary>
        /// Http client
        /// </summary>
        protected IHttpApiClient Client { get; private set; }
    }
}
