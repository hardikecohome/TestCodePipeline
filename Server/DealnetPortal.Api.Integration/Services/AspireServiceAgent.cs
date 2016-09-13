using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Models.Aspire;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireServiceAgent : IAspireServiceAgent
    {
        private IHttpApiClient AspireApiClient { get; set; }
        private readonly string _fullUri;

        public AspireServiceAgent(IHttpApiClient aspireClient)
        {
            AspireApiClient = aspireClient;
            _fullUri = AspireApiClient.Client.BaseAddress.ToString();
        }

        public async Task<HttpResponseMessage> DealUploadSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api/dealuploader/DealUploadSubmission.aspx
            //return await AspireApiClient.PostAsync(string.Format("{0}/dealuploader/DealUploadSubmission.aspx", _fullUri), dealUploadRequest, cancellationToken);
            throw new NotImplementedException();
        }
    }
}
