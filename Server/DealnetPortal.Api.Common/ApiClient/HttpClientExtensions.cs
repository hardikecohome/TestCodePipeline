using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.ApiClient
{
    public static class HttpExtensions
    {
        public static async Task<HttpResponseMessage> PostAsXmlWithSerializerAsync<T>(this HttpClient client, string requestUri, T value, CancellationToken cancellationToken)
        {
            return await client.PostAsync(requestUri, value,
                          new XmlMediaTypeFormatter { UseXmlSerializer = true },
                          cancellationToken).ConfigureAwait(false);
        }
    }
}
