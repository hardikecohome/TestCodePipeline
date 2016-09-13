using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Aspire;

namespace DealnetPortal.Api.Integration.Services
{
    /// <summary>
    /// Service agent for communicate with ASPIRE API
    /// </summary>
    public interface IAspireServiceAgent
    {
        /// <summary>
        /// Call deal upload submission
        /// </summary>
        /// <param name="dealUploadRequest"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> DealUploadSubmission(DealUploadRequest dealUploadRequest);
    }
}
