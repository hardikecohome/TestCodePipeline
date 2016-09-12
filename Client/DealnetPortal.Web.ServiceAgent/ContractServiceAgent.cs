using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities;

namespace DealnetPortal.Web.ServiceAgent
{
    public class ContractServiceAgent : ApiBase, IContractServiceAgent
    {
        private const string ContractApi = "Contract";
        private ILoggingService _loggingService;

        public ContractServiceAgent(IHttpApiClient client, ILoggingService loggingService)
            : base(client, ContractApi)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContract()
        {
            return await Client.PutAsync<string, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}/CreateContract", "");
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> GetContract(int contractId)
        {
            var alerts = new List<Alert>();
            try
            {
                var contract = await Client.GetAsync<ContractDTO>($"{_fullUri}/{contractId}");
                return new Tuple<ContractDTO, IList<Alert>>(contract, alerts);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Can't get contract with id {contractId}",
                    Message = ex.Message
                });
                _loggingService.LogError($"Can't get contract with id {contractId}", ex);
            }
            return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
        }

        public async Task<IList<ContractDTO>> GetContracts()
        {
            try
            {            
                return await Client.GetAsync<IList<ContractDTO>>(_fullUri);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts for an user", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateContractClientData(ContractDTO contract)
        {
            try
            {
                return
                    await Client.PutAsync<ContractDTO, IList<Alert>>($"{_fullUri}/UpdateContractClientData", contract);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't update client data for contract {contract.Id}", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId)
        {
            try
            {
                return
                    await
                        Client.PutAsync<string, IList<Alert>>(
                            $"{_fullUri}/InitiateCreditCheck?contractId={contractId}", "");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't initiate credit check for contract {contractId}", ex);
                throw;
            }
        }

        public async Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsync<Tuple<CreditCheckDTO, IList<Alert>>>(
                            $"{_fullUri}/GetCreditCheckResult?contractId={contractId}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't get credit check result for contract {contractId}", ex);
                throw;
            }
        }

        public async Task<IList<FlowingSummaryItemDTO>> GetContractsSummary(string summaryType)
        {
            try
            {            
                IList<FlowingSummaryItemDTO> result = await Client.GetAsync<IList<FlowingSummaryItemDTO>>($"{_fullUri}/{summaryType}/ContractsSummary");
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get credit contracts summary", ex);
                throw;
            }
        }
    }
}
