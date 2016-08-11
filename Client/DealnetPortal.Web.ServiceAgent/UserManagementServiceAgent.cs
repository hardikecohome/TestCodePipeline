using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Common.Api;

namespace DealnetPortal.Web.ServiceAgent
{
    public class UserManagementServiceAgent : ApiBase, IUserManagementServiceAgent
    {
        private const string AccountApi = "Account";

        public UserManagementServiceAgent(IHttpApiClient client)
            : base(client, AccountApi)
        {
        }

        public async Task<bool> Logout()
        {
            var result = await Client.Client.PostAsync(string.Format("{0}/Logout", _fullUri), null);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> Register(DealnetPortal.Api.Models.RegisterBindingModel registerModel)
        {
            var result = await Client.PostAsyncWithHttpResponse(string.Format("{0}/Register", _fullUri), registerModel);
            return result.IsSuccessStatusCode;
        }

        public async Task<bool> ChangePassword(DealnetPortal.Api.Models.ChangePasswordBindingModel changePasswordModel)
        {
            var result = await Client.PostAsyncWithHttpResponse(string.Format("{0}/ChangePassword", _fullUri), changePasswordModel);
            return result.IsSuccessStatusCode;
        }
    }
}
