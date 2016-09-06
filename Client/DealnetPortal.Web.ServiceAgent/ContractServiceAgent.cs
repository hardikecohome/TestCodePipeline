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
                var contract = await Client.GetAsync<ContractDTO>($"{_fullUri}/CreateContract");
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

        public Task<Tuple<ContractDTO, IList<Alert>>> GetContract(int contractId)
        {
            throw new NotImplementedException();
        }

        public Task<IList<ContractDTO>> GetContracts()
        {
            throw new NotImplementedException();
        }

        public Task<IList<Alert>> UpdateContractClientData(int contractId, IList<ContractAddressDTO> addresses, IList<CustomerDTO> customers)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Alert>> InitiateCreditCheck(int contractId)
        {
            throw new NotImplementedException();
        }

        public Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId)
        {
            throw new NotImplementedException();
        }
    }
}
