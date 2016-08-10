using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
    }
}
