using System.Threading;
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

        public async Task<DealUploadResponse> DealUploadSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api/dealuploader/DealUploadSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<DealUploadRequest, DealUploadResponse>($"{_fullUri}/dealuploader/DealUploadSubmission.aspx", dealUploadRequest, cancellationToken);
        }

        public async Task<DecisionCustomerResponse> CustomerUploadSubmission(CustomerRequest customerRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api//DealUploadWeb/CustomerUploadSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<CustomerRequest, DecisionCustomerResponse>($"{_fullUri}/DealUploadWeb/CustomerUploadSubmission.aspx", customerRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DealUploadResponse> CreditCheckSubmission(CreditCheckRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();

            //api//DealUploadWeb/CreditCheckSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<CreditCheckRequest, DealUploadResponse>($"{_fullUri}/DealUploadWeb/CreditCheckSubmission.aspx", dealUploadRequest, cancellationToken).ConfigureAwait(false);
        }

        public async Task<DecisionLoginResponse> LoginSubmission(DealUploadRequest dealUploadRequest)
        {
            CancellationToken cancellationToken = new CancellationToken();
            //api//DealUploadWeb/LoginSubmission.aspx
            return await AspireApiClient.PostAsyncXmlWithXmlResponce<DealUploadRequest, DecisionLoginResponse>($"{_fullUri}/DealUploadWeb/LoginSubmission.aspx", dealUploadRequest, cancellationToken).ConfigureAwait(false);
        }
    }
}
