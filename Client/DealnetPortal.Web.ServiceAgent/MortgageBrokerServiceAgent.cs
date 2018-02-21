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
    public class MortgageBrokerServiceAgent : ApiBase, IMortgageBrokerServiceAgent
    {
        private const string MortgageBrokerApi = "MortgageBroker";
        private readonly ILoggingService _loggingService;

        public MortgageBrokerServiceAgent(IHttpApiClient client, ILoggingService loggingService, IAuthenticationManager authenticationManager) 
            : base(client, MortgageBrokerApi, authenticationManager)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(NewCustomerDTO customerForm)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<NewCustomerDTO, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}", customerForm, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit New Customer", ex);
                throw;
            }
        }

        public async Task<IList<ContractDTO>> GetCreatedContracts()
        {
            try
            {
                return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts created by an user", ex);
                return new List<ContractDTO>();
            }
        }
    }
}
