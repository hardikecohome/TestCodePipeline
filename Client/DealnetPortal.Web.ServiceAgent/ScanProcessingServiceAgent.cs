using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Helpers;
using Microsoft.Owin.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace DealnetPortal.Web.ServiceAgent
{
    public class ScanProcessingServiceAgent : ApiBase, IScanProcessingServiceAgent
    {
        private const string AgentApi = "ScanProcessing";
        public ScanProcessingServiceAgent(IHttpApiClient client, IAuthenticationManager authenticationManager) 
            : base(client, AgentApi, authenticationManager)
        {
        }

        public async Task<Tuple<DriverLicenseData, IList<Alert>>> ScanDriverLicense(ScanningRequest scanningRequest)
        {           
            MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[] { bsonFormatter, };

            var result = await Client.PostAsyncEx<ScanningRequest, Tuple<DriverLicenseData, IList<Alert>>>($"{_fullUri}/PostLicenseScanProcessing", scanningRequest, 
                AuthenticationHeader, null, bsonFormatter);
            return result;
            //return await result.Content.ReadAsAsync<Tuple<DriverLicenseData, IList<Alert>>>(formatters);
        }

        public async Task<Tuple<VoidChequeData, IList<Alert>>> ScanVoidCheque(ScanningRequest scanningRequest)
        {
            MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[] { bsonFormatter, };

            var result = await Client.PostAsyncEx<ScanningRequest, Tuple<VoidChequeData, IList<Alert>>>($"{_fullUri}/PostChequeScanProcessing", scanningRequest, 
                AuthenticationHeader, null, bsonFormatter);
            return result;
            //return await result.Content.ReadAsAsync<Tuple<VoidChequeData, IList<Alert>>>(formatters);
        }
    }
}
