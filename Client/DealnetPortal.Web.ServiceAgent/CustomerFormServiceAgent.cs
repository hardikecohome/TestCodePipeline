using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                            $"{_fullUri}/SubmitCustomerForm", customerForm, null, CurrentCulture);
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
                        $"{_fullUri}/GetCustomerContractInfo?contractId={contractId}&dealerName={dealerName}", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit Customer contract info", ex);
                throw;
            }
        }
    }
}
