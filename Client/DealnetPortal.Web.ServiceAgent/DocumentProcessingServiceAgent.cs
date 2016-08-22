using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Common.Api;

namespace DealnetPortal.Web.ServiceAgent
{
    public class DocumentProcessingServiceAgent : ApiBase, IDocumentProcessingServiceAgent
    {
        private const string AgentApi = "DocumentProcessing";
        public DocumentProcessingServiceAgent(IHttpApiClient client) 
            : base(client, AgentApi)
        {
        }

        public async Task<Tuple<DriverLicenseData, IList<Alert>>> GetDriverLicense(ScanningRequest scanningRequest)
        {
            return await Client.PostAsync<ScanningRequest, Tuple<DriverLicenseData, IList<Alert>>>(_uri + "/GetDriverLicense", scanningRequest);
        }
    }
}
