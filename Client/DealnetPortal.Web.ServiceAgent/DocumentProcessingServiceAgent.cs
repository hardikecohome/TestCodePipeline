using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace DealnetPortal.Web.ServiceAgent
{
    public class DocumentProcessingServiceAgent : ApiBase, IDocumentProcessingServiceAgent
    {
        private const string AgentApi = "DocumentProcessing";
        public DocumentProcessingServiceAgent(IHttpApiClient client) 
            : base(client, AgentApi)
        {
        }

        public async Task<HttpResponseMessage> GetDriverLicense(ScanningRequest scanningRequest)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:37679")
            };

            scanningRequest.OperationId = "1";
            scanningRequest.ImageForReadRaw = null;
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/bson"));
            MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
            //var result = await client.PostAsync<ScanningRequest>("api/DocumentProcessing", scanningRequest, bsonFormatter);
            var result = await client.GetAsync("api/DocumentProcessing/1");
            result.EnsureSuccessStatusCode();


            //var content = new MultipartFormDataContent();
            //var binaryContent = new ByteArrayContent(scanningRequest.ImageForReadRaw);
            //binaryContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            //content.Add(binaryContent, "ImageForReadRaw");
            //var response = await Client.PostAsyncWithHttpResponse($"{_fullUri}/DocumentProcessing", content);
            ////var result = response;

            // Set the Accept header for BSON.
            //Client.Client.DefaultRequestHeaders.Accept.Clear();
            //Client.Client.DefaultRequestHeaders.Accept.Add(
            //        new MediaTypeWithQualityHeaderValue("application/bson"));            

            //// POST using the BSON formatter.
            //MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
            //var result = await Client.Client.PostAsync<ScanningRequest>($"{_fullUri}/PostDriverLicense", scanningRequest, bsonFormatter);



            //Client.Client.DefaultRequestHeaders.Accept.Clear();
            //Client.Client.DefaultRequestHeaders.Accept.Add(
            //        new MediaTypeWithQualityHeaderValue("application/bson"));

            //using (var stream = new MemoryStream())
            //using (var bson = new BsonWriter(stream))
            //{
            //    var jsonSerializer = new JsonSerializer();

            //    jsonSerializer.Serialize(bson, scanningRequest);

            //    var byteArrayContent = new ByteArrayContent(stream.ToArray());
            //    byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue("application/bson");

            //    var res = await Client.Client.PostAsync(
            //            $"{_fullUri}/PostDriverLicense", byteArrayContent);

            //    if (!res.IsSuccessStatusCode)
            //    {

            //    }

            //}

            //if (!response.IsSuccessStatusCode)
            {
                //HttpResponseHelpers
            }

            return null;
            //return await Client.PostAsync<ScanningRequest, Tuple<DriverLicenseData, IList<Alert>>>($"{_fullUri}/GetDriverLicense", scanningRequest);
        }
    }
}
