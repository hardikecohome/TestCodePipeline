using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Enumeration;
using DealnetPortal.Web.Common.Api;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Models;
using Microsoft.Practices.ObjectBuilder2;
using Newtonsoft.Json;

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

        public async Task<IList<Alert>> Register(DealnetPortal.Api.Models.RegisterBindingModel registerModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse(string.Format("{0}/Register", _fullUri), registerModel);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> ChangePassword(DealnetPortal.Api.Models.ChangePasswordBindingModel changePasswordModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse(string.Format("{0}/ChangePassword", _fullUri), changePasswordModel);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }
    }    
}

