using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Common.Api;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DealnetPortal.Web.ServiceAgent
{
    public class SecurityServiceAgent : ApiBase, ISecurityServiceAgent
    {
        private readonly Uri _loginUri;

        public SecurityServiceAgent(IHttpApiClient client)
            : base(client, string.Empty)
        {
            var baseUri = Client.Client.BaseAddress;
            _loginUri = new Uri(baseUri.GetLeftPart(UriPartial.Authority) + "/Token");
        }

        /// <summary>
        /// Authenicate user 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>User Principal </returns>
        public async Task<IPrincipal> Authenicate(string userName, string password)
        {
            IPrincipal user = null;

            var data = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", userName),
                new KeyValuePair<string, string>("Password", password)
            });

            try
            {
                var response = await Client.Client.PostAsync(_loginUri.AbsoluteUri, data);
                var results =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(
                        response.Content.ReadAsStringAsync().Result);

                if (!results.ContainsKey("access_token"))
                    return null;

                IEnumerable<string> userData;
                if (response.Headers.TryGetValues("user", out userData))
                {
                    JObject userProxy = JsonConvert.DeserializeObject<dynamic>(userData.First());

                    var coursesTokens = userProxy.SelectTokens("Claims[*]").ToList();

                    //var claims = coursesTokens
                    //    .Select(coursesToken => JsonConvert.DeserializeObject<ServerClaim>(coursesToken.ToString()))
                    //    .Select(serverClaim => new Claim(serverClaim.ClaimType, serverClaim.ClaimValue)).ToList();


                    //claims.Add(new Claim(ClaimTypes.Name, userProxy.SelectTokens("UserName").Select(s => s.ToString()).First()));
                }

            }
            catch (Exception ex)
            {
                //TODO: log error
                return null;
            }

            return user;
        }

        public void SetAuthorizationHeader(IPrincipal principal)
        {
            throw new NotImplementedException();
        }
    }
}
