using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities;

namespace DealnetPortal.Web.ServiceAgent
{
    public class StorageServiceAgent : ApiBase, IStorageServiceAgent
    {
        private const string StorageApi = "Storage";
        private ILoggingService _loggingService;

        public StorageServiceAgent(ICustomHttpApiClient client, ILoggingService loggingService) 
            : base(client, StorageApi)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<AgreementTemplateDTO, IList<Alert>>> UploadAgreementTemplate(AgreementTemplateDTO newAgreementTemplate)
        {
            var alerts = new List<Alert>();
            AgreementTemplateDTO addedAgreement = null;
            try
            {
                MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
                MediaTypeFormatter[] formatters = new MediaTypeFormatter[] {bsonFormatter,};

                var result =
                    await
                        Client.Client.PostAsync<AgreementTemplateDTO>($"{_fullUri}/UploadAgreementTemplate",
                            newAgreementTemplate, bsonFormatter);
                result.EnsureSuccessStatusCode();
                addedAgreement = await result.Content.ReadAsAsync<AgreementTemplateDTO>(formatters);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Can't add new agreement template",
                    Message = ex.Message
                });
                _loggingService.LogError("Can't add new agreement template");
            }
            return new Tuple<AgreementTemplateDTO, IList<Alert>>(addedAgreement, alerts);
        }
    }
}
