using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;

namespace DealnetPortal.Api.Integration.ServiceAgents
{
    public class CustomerWalletServiceAgent : ICustomerWalletServiceAgent
    {
        private IHttpApiClient AspireApiClient { get; set; }
        private readonly string _fullUri;

        public CustomerWalletServiceAgent(IHttpApiClient aspireClient)
        {
            AspireApiClient = aspireClient;
            _fullUri = AspireApiClient.Client.BaseAddress.ToString();
        }
    }
}
