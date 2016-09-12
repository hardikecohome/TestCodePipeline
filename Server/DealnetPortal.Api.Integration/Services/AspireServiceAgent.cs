using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireServiceAgent : IAspireServiceAgent
    {
        private IHttpApiClient AspireApiClient { get; set; }

        public AspireServiceAgent(IHttpApiClient aspireClient)
        {
            AspireApiClient = aspireClient;
        }
    }
}
