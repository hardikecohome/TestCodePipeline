using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Api
{
    public class HttpApiClient : IHttpApiClient
    {
        public HttpApiClient(string baseAddress)
        {            
            Client = new HttpClient(new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            })
            {
                BaseAddress = new Uri(baseAddress),
                Timeout = Timeout.InfiniteTimeSpan
            };
        }

        public Uri ConstructUrl(string requestUri)
        {
            return new Uri(Client.BaseAddress, requestUri);
        }

        public HttpClient Client { get; private set; }
    }
}
