using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Helpers;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.ServiceAgent
{
    public class UserManagementServiceAgent : ApiBase, IUserManagementServiceAgent
    {
        private const string AccountApi = "Account";

        public UserManagementServiceAgent(IHttpApiClient client, IAuthenticationManager authenticationManager)
            : base(client, AccountApi, authenticationManager)
        {
        }

        public async Task<bool> Logout()
        {
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/Logout", "", AuthenticationHeader);
            return result.IsSuccessStatusCode;
        }

        public async Task<IList<Alert>> Register(Api.Models.RegisterBindingModel registerModel)
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

        public async Task<IList<Alert>> ChangePassword(Api.Models.ChangePasswordBindingModel changePasswordModel)
        {
            var alerts = new List<Alert>();
            var result = await Client.PostAsyncWithHttpResponse($"{_fullUri}/ChangePassword", changePasswordModel, AuthenticationHeader);

            if (!result.IsSuccessStatusCode)
            {
                var errorAlerts = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                alerts.AddRange(errorAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> ChangePasswordAnonymously(Api.Models.ChangePasswordAnonymouslyBindingModel changePasswordModel)
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

        public async Task<IList<Alert>> ForgotPassword(Api.Models.ForgotPasswordBindingModel forgotPasswordModel)
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

