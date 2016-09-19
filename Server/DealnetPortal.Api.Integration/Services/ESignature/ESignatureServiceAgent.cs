using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Aspire;
using DealnetPortal.Utilities;

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
            ReadCookies(response);
            response.EnsureSuccessStatusCode();

            return alerts;
        }

        public async Task<bool> Logout()
        {
            var response = await Client.Client.PostAsync(_fullUri + "/eoLogout", null);
            response.EnsureSuccessStatusCode();
            return response.IsSuccessStatusCode;
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
            return Client.Cookies;
        }
    }
}
