using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common.Api;

namespace DealnetPortal.Web.ServiceAgent
{
    public class ContractServiceAgent : ApiBase, IContractServiceAgent
    {
        private const string ContractApi = "Contract";

        public ContractServiceAgent(IHttpApiClient client)
            : base(client, ContractApi)
        {            
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContract()
        {
            var alerts = new List<Alert>();
            try
            {            
                var contract = await Client.PutAsync<string, ContractDTO>($"{_fullUri}/CreateContract","");
                return new Tuple<ContractDTO, IList<Alert>>(contract, alerts);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractCreateFailed,
                    Message = ex.Message
                });
            }
            return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
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
            }
            return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
        }

        public async Task<IList<ContractDTO>> GetContracts()
        {
            return await Client.GetAsync<IList<ContractDTO>>(_fullUri);
        }

        public async Task<IList<Alert>> UpdateContractClientData(ContractDTO contract)
        {
            return await Client.PutAsync<ContractDTO, IList<Alert>>($"{_fullUri}/UpdateContractClientData", contract);
        }

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId)
        {
            return await Client.PutAsync<string, IList<Alert>>($"{_fullUri}/InitiateCreditCheck?contractId={contractId}", "");
        }

        public async Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId)
        {
            return await Client.GetAsync<Tuple<CreditCheckDTO, IList<Alert>>>($"{_fullUri}/GetCreditCheckResult?contractId={contractId}");
        }
    }
}
