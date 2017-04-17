using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Aspire;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Common.Types;
using DealnetPortal.Web.Models;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DealnetPortal.Web.ServiceAgent
{
    public class SecurityServiceAgent : ApiBase, ISecurityServiceAgent
    {
        private readonly Uri _loginUri;
        private readonly ILoggingService _loggingService;

        public SecurityServiceAgent(IHttpApiClient client, ILoggingService loggingService)
            : base(client, string.Empty)
        {
            _loggingService = loggingService;
            var baseUri = Client.Client.BaseAddress;
            try
            {
                _loginUri = new Uri(baseUri.AbsoluteUri.Replace("/api", "") + "/Token");
            }
            catch (Exception)
            {
                _loginUri = new Uri(baseUri.GetLeftPart(UriPartial.Authority) + "/Token");
            }
        }    

        /// <summary>
        /// Authenicate user 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>User Principal </returns>
        public async Task<Tuple<IPrincipal, IList<Alert>>> Authenicate(string userName, string password, string portalId)
        {
            //Tuple<IPrincipal, IList<Alert>> result = new Tuple<IPrincipal, IList<Alert>>(null, new List<Alert>());
            IList<Alert> alerts = new List<Alert>();
            IPrincipal user = null;

            var data = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("portalId", portalId)
            });

            try
            {
                var response = await Client.Client.PostAsync(_loginUri.AbsoluteUri, data);

                if (response.IsSuccessStatusCode)
                {
                    var results =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(
                            response.Content.ReadAsStringAsync().Result);

                    if (!results.ContainsKey("access_token"))
                        return null;

                    var claims = new List<Claim>();
                    string returnedName;
                    if (results.TryGetValue("userName", out returnedName))
                    {
                        claims.Add(new Claim(ClaimTypes.Name, returnedName));
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Name, userName));
                    }

                    if (results.ContainsKey("roles"))
                    {
                        results["roles"].Split(':').ForEach(r => claims.Add(new Claim(ClaimTypes.Role, r)));
                    }

                    results.Where(r => r.Key.Contains("claim:")).ForEach(c =>
                    {                        
                        var claimType = c.Key.Substring("claim:".Length);
                        if (!string.IsNullOrEmpty(claimType))
                        {
                            claims.Add(new Claim(claimType, c.Value));
                        }
                    });

                    var identity = new UserIdentity(claims) {Token = results["access_token"]};
                    user = new UserPrincipal(identity);
                }
                else
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result =
                            JsonConvert.DeserializeObject<OAuthErrorResponseModel>(responseContent);
                    if (result != null)
                    {
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Error,
                            Header = result.Error,
                            Message = result.Error_Description
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "An error occurred during authentication",
                    Message = ex.ToString(),                    
                });
                _loggingService.LogError("An error occurred during authentication", ex);                
                //return null;
            }
            return new Tuple<IPrincipal, IList<Alert>>(user, alerts);
        }

        /// <summary>
        /// Set Authorization header
        /// </summary>
        /// <param name="principal">user</param>
        public void SetAuthorizationHeader(IPrincipal principal)
        {
            //Set Authorization header
            Client.Client.DefaultRequestHeaders.Authorization =
               new AuthenticationHeaderValue("Bearer", ((UserIdentity)principal?.Identity)?.Token ?? string.Empty);
        }

        public bool IsAutorizated()
        {
            var auth = Client.Client?.DefaultRequestHeaders?.Authorization;
            if (auth != null && auth.Scheme == "Bearer" && !string.IsNullOrEmpty(auth.Parameter))
            {
                return true;
            }
            return false;
        }
    }
}
