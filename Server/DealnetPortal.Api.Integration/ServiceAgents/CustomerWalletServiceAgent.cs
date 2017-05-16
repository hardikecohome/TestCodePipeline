using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.CustomerWallet;

namespace DealnetPortal.Api.Integration.ServiceAgents
{
    public class CustomerWalletServiceAgent : ICustomerWalletServiceAgent
    {
        private IHttpApiClient Client { get; set; }
        private readonly string _fullUri;

        public CustomerWalletServiceAgent(IHttpApiClient client)
        {
            Client = client;            
            _fullUri = Client.Client.BaseAddress.ToString();
        }

        public async Task<IList<Alert>> RegisterCustomer(RegisterCustomerBindingModel registerCustomer)
        {
            var alerts = new List<Alert>();
            try
            {
                return await Client.PostAsync<RegisterCustomerBindingModel, IList<Alert>>(
                            $"{_fullUri}/Account/RegisterCustomer", registerCustomer);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Register new customer on Customer Wallet portal failed",
                    Message = ex.Message
                });
            }
            return new List<Alert>(alerts);
        }

    }
}
