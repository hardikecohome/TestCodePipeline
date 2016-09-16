using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace DealnetPortal.Web.ServiceAgent
{
    public class ScanProcessingServiceAgent : ApiBase, IScanProcessingServiceAgent
    {
        private const string AgentApi = "ScanProcessing";
        public ScanProcessingServiceAgent(IHttpApiClient client) 
            : base(client, AgentApi)
        {
        }

        public async Task<Tuple<DriverLicenseData, IList<Alert>>> ScanDriverLicense(ScanningRequest scanningRequest)
        {
            //DriverLicenseData driverLicenseData = null;
            //List<Alert> alerts = new List<Alert>();            
            //client.DefaultRequestHeaders.Accept.Clear();
            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/bson"));
            MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
            MediaTypeFormatter[] formatters = new MediaTypeFormatter[] { bsonFormatter, };

            var result = await Client.Client.PostAsync<ScanningRequest>(_fullUri, scanningRequest, bsonFormatter);

            return await result.Content.ReadAsAsync<Tuple<DriverLicenseData, IList<Alert>>>(formatters);
        }
    }
}
