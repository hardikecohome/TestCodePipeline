using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.ServiceAgent
{
    public class CustomerFormServiceAgent : ApiBase, ICustomerFormServiceAgent
    {
        private const string ContractApi = "CustomerForm";
        private readonly ILoggingService _loggingService;

        public CustomerFormServiceAgent(IHttpApiClient client, ILoggingService loggingService, IAuthenticationManager authenticationManager)
            : base(client, ContractApi, authenticationManager)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormDTO customerForm)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<CustomerFormDTO, Tuple<CustomerContractInfoDTO, IList<Alert>>>(
                            $"{_fullUri}", customerForm, null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit Customer Form", ex);
                throw;
            }
        }

        public async Task<CustomerContractInfoDTO> GetCustomerContractInfo(int contractId, string dealerName)
        {
            try
            {
                return await Client.GetAsyncEx<CustomerContractInfoDTO>(
                        $"{_fullUri}/{contractId}/{dealerName}", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get submitted Customer contract info", ex);
                throw;
            }
        }

        public async Task<CustomerLinkDTO> GetShareableLinkSettings()
        {
            try
            {
                return await Client.GetAsyncEx<CustomerLinkDTO>(
                    $"{_fullUri}/Settings", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Customer Link Settings", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateShareableLinkSettings(CustomerLinkDTO customerLink)
        {
            try
            {
                return await Client.PutAsyncEx<CustomerLinkDTO, IList<Alert>>(
                    $"{_fullUri}/Settings", customerLink, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't change Customer Link Settings", ex);
                throw;
            }
        }

        public async Task<CustomerLinkLanguageOptionsDTO> GetCustomerLinkLanguageOptions(string hashDealerName, string culture)
        {
            try
            {
                return await Client.GetAsync<CustomerLinkLanguageOptionsDTO>(
                    $"{_fullUri}/LinkOptions/{hashDealerName}/{culture}").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get Customer Link Language Options", ex);
                throw;
            }
        }
    }
}
