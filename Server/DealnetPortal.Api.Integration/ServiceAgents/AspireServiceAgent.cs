﻿using System.Threading;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Models.Aspire;

namespace DealnetPortal.Api.Integration.ServiceAgents
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

        public async Task<DealUploadResponce> DealUploadSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api/dealuploader/DealUploadSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<DealUploadRequest, DealUploadResponce>($"{_fullUri}/dealuploader/DealUploadSubmission.aspx", dealUploadRequest, cancellationToken);
        }

        public async Task<DealUploadResponce> CustomerUploadSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api//DealUploadWeb/CustomerUploadSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<DealUploadRequest, DealUploadResponce>($"{_fullUri}/DealUploadWeb/CustomerUploadSubmission.aspx", dealUploadRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DealUploadResponce> CreditCheckSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api//DealUploadWeb/CreditCheckSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<DealUploadRequest, DealUploadResponce>($"{_fullUri}/DealUploadWeb/CreditCheckSubmission.aspx", dealUploadRequest, cancellationToken).ConfigureAwait(false);
        }
    }
}
