using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using DealnetPortal.Api.Common.Helpers;

namespace DealnetPortal.Api.Common.ApiClient
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
            //Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/bson"));
        }

        public Uri ConstructUrl(string requestUri)
        {
            return new Uri(Client.BaseAddress, requestUri);
        }

        public HttpClient Client { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>This operation will retry</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> PostAsync<T>(string requestUri, T content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var response = await Client.PostAsJsonAsync(requestUri, content, cancellationToken);

                if (response?.Content == null)
                    return default(T);
                return await response.Content.ReadAsAsync<T>(cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        public async Task<T2> PostAsync<T1, T2>(string requestUri, T1 content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var response = await Client.PostAsJsonAsync(requestUri, content, cancellationToken);

                if (response?.Content == null)
                    return default(T2);

                return await response.Content.ReadAsAsync<T2>(cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        public async Task<HttpResponseMessage> PostAsyncWithHttpResponse<T>(string requestUri, T content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await Client.PostAsJsonAsync(requestUri, content, cancellationToken);
        }

        public async Task<T2> PostAsyncXmlWithXmlResponce<T1, T2>(string requestUri, T1 content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var response = await Client.PostAsXmlWithSerializerAsync(requestUri, content, cancellationToken);

                if (response?.Content == null)
                    return default(T2);
                return new XmlSerializerHelper().DeserializeFromString<T2>(await response.Content.ReadAsStringAsync());
                //return await response.Content.ReadAsAsync<T2>(new [] { new XmlMediaTypeFormatter { UseXmlSerializer = true }}, cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        /// <summary>
        /// See <see cref="IHttpApiClient"/>
        /// </summary>
        /// <remarks>This operation will retry.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string requestUri,
            CancellationToken cancellationToken = new CancellationToken())
        {
            HttpResponseMessage response = await Client.GetAsync(requestUri, cancellationToken);

            if (response == null || response.Content == null)
                return default(T);

            return await response.Content.ReadAsAsync<T>(cancellationToken);
        }

        /// <summary>
        /// Perform a get operation against a uri synchronously.
        /// </summary>
        /// <typeparam name="T">Type of the content or model.</typeparam>
        /// <param name="requestUri">Uri of resource</param>
        /// <returns>Model or resource from the Get operation against the uri.</returns>
        public T Get<T>(string requestUri)
        {
            var response = Client.GetAsync(requestUri).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content;

                // by calling .Result you are synchronously reading the result
                return responseContent.ReadAsAsync<T>().Result;
            }
            else
                return default(T);
        }

        /// <summary>
        /// See <see cref="IHttpApiClient"/>
        /// </summary>
        /// <remarks>This operation will retry.</remarks>
        /// <typeparam name="T"></typeparam>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T> PutAsync<T>(string requestUri, T content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var response = await Client.PutAsJsonAsync(requestUri, content, cancellationToken);
                return await response.Content.ReadAsAsync<T>(cancellationToken);
            }
            catch (HttpRequestException)
            {
                throw;
            }
        }

        /// <summary>
        /// See <see cref="IHttpApiClient"/>
        /// </summary>
        /// <remarks>This operation will retry</remarks>
        /// <typeparam name="T1">Type of the request</typeparam>
        /// <typeparam name="T2">Type of the response</typeparam>
        /// <param name="requestUri"></param>
        /// <param name="content"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<T2> PutAsync<T1, T2>(string requestUri, T1 content,
            CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var response = await Client.PutAsJsonAsync(requestUri, content, cancellationToken);

                if (response?.Content == null)
                    return default(T2);

                return await response.Content.ReadAsAsync<T2>(cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                throw;
            }
        }
    }
}
