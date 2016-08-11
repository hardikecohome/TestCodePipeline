using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Api
{
    /// <summary>
    /// Client for interacting with REST or Http based services.
    /// </summary>
    public interface IHttpApiClient
    {
        /// <summary>
        /// Constructs Url using client`s BaseAddress and relative path in requestUri parameter
        /// </summary>
        /// <param name="requestUri">relative server path</param>
        /// <returns>constructed Url</returns>
        Uri ConstructUrl(string requestUri);

        /// <summary>
        /// Base HttpClient 
        /// </summary>
        HttpClient Client { get; }

        /// <summary>
        /// Perform a post operation against a uri.
        /// </summary>
        /// <typeparam name="T">Type of the content or model.</typeparam>
        /// <param name="requestUri">Uri of resource</param>
        /// <param name="content">Individual or list of resources to post.</param>
        /// <param name="cancellationToken">Allows clients to cancel a request.</param>
        /// <returns>Model or resource from the Post operation against the uri.</returns>
        Task<T> PostAsync<T>(string requestUri, T content, CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// Perform a post operation against a uri.
        /// </summary>
        /// <typeparam name="T1">Type of the request</typeparam>
        /// <typeparam name="T2">Type of the response</typeparam>
        /// <param name="requestUri">Uri of resource</param>
        /// <param name="content">Individual or list of resources to post.</param>
        /// <param name="cancellationToken">Allows clients to cancel a request.</param>
        /// <returns>Model or resource from the Post operation against the uri.</returns>
        Task<T2> PostAsync<T1, T2>(string requestUri, T1 content, CancellationToken cancellationToken = new CancellationToken());

        /// <summary>
        /// Perform a post operation against a uri.
        /// </summary>
        /// <typeparam name="T">Type of the content or model.</typeparam>
        /// <param name="requestUri">Uri of resource</param>
        /// <param name="content">array of resources to post.</param>
        /// <param name="cancellationToken">Allows clients to cancel a request.</param>
        /// <returns>The response and result from the api.</returns>
        Task<HttpResponseMessage> PostAsyncWithHttpResponse<T>(string requestUri, T content, CancellationToken cancellationToken = new CancellationToken());
    }
}
