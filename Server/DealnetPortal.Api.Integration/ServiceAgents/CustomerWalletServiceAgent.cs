﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.CustomerWallet;
using Microsoft.Practices.ObjectBuilder2;

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
                var result =
                    await Client.PostAsyncWithHttpResponse($"{_fullUri}/Account/RegisterCustomer", registerCustomer);
                if (!result.IsSuccessStatusCode)
                {
                    var errors = await HttpResponseHelpers.GetModelStateErrorsAsync(result.Content);
                    errors.ModelState?.ForEach(st => st.Value.ForEach(val =>
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Error,
                            Message = val,
                            Header = st.Key
                        })));
                }
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
            return alerts;
        }

        public async Task<IList<Alert>> CreateTransaction(TransactionInfoDTO transactionInfo)
        {
            var alerts = new List<Alert>();
            try
            {
                return
                    await
                        Client.PutAsync<TransactionInfoDTO, IList<Alert>>($"{_fullUri}/Customer/CreateTransaction",
                            transactionInfo);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Creation of a new transaction on Customer Wallet portal failed",
                    Message = ex.Message
                });
            }
            return alerts;
        }

        public Task<bool> CheckUser(string userName)
        {
            try
            {
                return Client.GetAsync<bool>(
                            $"{_fullUri}/Account/CheckUser?userName={userName}");
            }
            catch (Exception ex)
            {                
                throw;
            }
        }
    }
}