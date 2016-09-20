using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.Services.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Aspire;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services.ESignature
{
    public class ESignatureServiceAgent : IESignatureServiceAgent
    {
        private IHttpApiClient Client { get; set; }
        private ILoggingService LoggingService { get; set; }
        private readonly string _fullUri;
        public ESignatureServiceAgent(IHttpApiClient ecoreClient, ILoggingService loggingService)
        {
            Client = ecoreClient;
            LoggingService = loggingService;
            //AspireApiClient = aspireClient;
            _fullUri = string.Format("{0}/{1}", Client.Client.BaseAddress, "ecore");
        }


        public async Task<IList<Alert>> Login(string userName, string organisation, string password)
        {
            IList<Alert> alerts = new List<Alert>();
            var data = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("loginUsername", userName),
                new KeyValuePair<string, string>("loginOrganization", organisation),
                new KeyValuePair<string, string>("loginPassword", password)
            });

            var response = await Client.Client.PostAsync(_fullUri + "/?action=eoLogin", data);            
            response.EnsureSuccessStatusCode();

            if (response?.Content != null)
            {
                var eResponse = await response.Content.DeserializeFromStringAsync<EOriginalTypes.response>();
                if (eResponse?.status != responseStatus.ok)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.EcoreConnectionFailed,
                        Message = "Can't connect to eCore service"
                    });
                }
                else
                {
                    ReadCookies(response);
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.EcoreConnectionFailed,
                    Message = "Can't connect to eCore service"
                });
            }            

            return alerts;
        }

        public async Task<bool> Logout()
        {
            var response = await Client.Client.PostAsync(_fullUri + "/?action=eoLogout", null);
            response.EnsureSuccessStatusCode();
            var eResponse = await response.Content.DeserializeFromStringAsync<EOriginalTypes.response>();

            return eResponse.status == responseStatus.ok;
        }

        public async Task<bool> CreateTransaction(string transactionName)
        {
            IList<Alert> alerts = new List<Alert>();
            var data = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("transactionName", transactionName),                
            });            

            var response = await Client.Client.PostAsync(_fullUri + "/?action=eoCreateTransaction", data);

            var eResponse = await response.Content.DeserializeFromStringAsync<EOriginalTypes.response>();

            return eResponse.status == responseStatus.ok;
        }


        protected CookieContainer ReadCookies(HttpResponseMessage response)
        {
            var pageUri = response.RequestMessage.RequestUri;

            IEnumerable<string> cookies;
            //TODO: delete path
            if (response.Headers.TryGetValues("set-cookie", out cookies))
            {
                foreach (var c in cookies)
                {
                    var cookie = c;
                    var idx = c.IndexOf("Path", StringComparison.Ordinal);
                    if (idx > 0)
                    {
                        cookie = c.Substring(0, idx);
                    }
                    Client.Cookies.SetCookies(new Uri(_fullUri), cookie);
                }
            }
            return null;
            //return Client.Cookies;
        }

        private IList<Alert> GetAlertsFromResponse(EOriginalTypes.response response)
        {
            var alerts = new List<Alert>();

            response?.errorList?.ForEach(e =>
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Message = e.ItemsElementName.Contains(ItemsChoiceType16.message) ? e.Items[Array.IndexOf(e.ItemsElementName, ItemsChoiceType16.message)] : string.Empty,
                    Header = e.ItemsElementName.Contains(ItemsChoiceType16.minorCode) ? e.Items[Array.IndexOf(e.ItemsElementName, ItemsChoiceType16.minorCode)] : string.Empty,                    
                })
                );

            return alerts;
        }
    }
}
