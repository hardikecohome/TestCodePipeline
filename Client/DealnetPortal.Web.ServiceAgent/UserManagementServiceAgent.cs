using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Models;
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
            var result = await Client.Client.PostAsync($"{_fullUri}/Logout", null);
            return result.IsSuccessStatusCode;
        }

        public async Task<IList<Alert>> Register(DealnetPortal.Api.Models.RegisterBindingModel registerModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/Register", registerModel);

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
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/ChangePassword", changePasswordModel);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> ChangePasswordAnonymously(DealnetPortal.Api.Models.ChangePasswordAnonymouslyBindingModel changePasswordModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/ChangePasswordAnonymously", changePasswordModel);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> ForgotPassword(DealnetPortal.Api.Models.ForgotPasswordBindingModel forgotPasswordModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/ForgotPassword", forgotPasswordModel);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }
    }    
}

