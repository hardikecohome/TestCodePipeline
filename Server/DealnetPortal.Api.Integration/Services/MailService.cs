using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class MailService : IMailService
    {
        private readonly ILoggingService _loggingService;

        public MailService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task<IList<Alert>> SendSubmitNotification(ContractDTO contract, string dealerEmail)
        {
            var alerts = new List<Alert>();
            var id = contract.Details?.TransactionId ?? contract.Id.ToString();
            var subject = $"Contract {id} was successfully submitted";
            var body = new StringBuilder();
            body.AppendLine($"Contract {id} was successfully submitted");
            body.AppendLine($"Type of Application: {contract.Equipment.AgreementType}");
            body.AppendLine($"Home Owner's Name: {contract.PrimaryCustomer?.FirstName} {contract.PrimaryCustomer?.LastName}");
            await SendNotification(body.ToString(), subject, contract, dealerEmail, alerts);                

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }

            return alerts;
        }

        public async Task<IList<Alert>> SendChangeNotification(ContractDTO contract, string dealerEmail)
        {
            var alerts = new List<Alert>();
            
            var id = contract.Details?.TransactionId ?? contract.Id.ToString();
            var subject = $"Contract {id} was successfully changed";
            var body = new StringBuilder();
            body.AppendLine($"Contract {id} was successfully changed");
            body.AppendLine($"Type of Application: {contract.Equipment.AgreementType}");
            body.AppendLine(
                $"Home Owner's Name: {contract.PrimaryCustomer?.FirstName} {contract.PrimaryCustomer?.LastName}");
            await SendNotification(body.ToString(), subject, contract, dealerEmail, alerts);

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }

            return alerts;
        }

        private async Task SendNotification(string body, string subject, ContractDTO contract, string dealerEmail, List<Alert> alerts)
        {
            if (contract != null)
            {
                var recipients = GetContractRecipients(contract, dealerEmail);

                if (recipients.Any())
                {
                    try
                    {
                        foreach (var recipient in recipients)
                        {
                            var sAlerts = await SendEmail(new List<string>() { recipient }, subject, body.ToString());
                            if (sAlerts?.Any() ?? false)
                            {
                                alerts.AddRange(sAlerts);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Can't send notification email";
                        _loggingService.LogError("Can't send notification email", ex);
                        alerts.Add(new Alert()
                        {
                            Header = errorMsg,
                            Message = ex.ToString(),
                            Type = AlertType.Error
                        });
                    }
                }
                else
                {
                    var errorMsg = $"Can't get recipients list for contract [{contract.Id}]";
                    _loggingService.LogError(errorMsg);
                    alerts.Add(new Alert()
                    {
                        Header = "Can't get recipients list",
                        Message = errorMsg,
                        Type = AlertType.Error
                    });
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contract.Id}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contract.Id}");
            }

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }
        }

        private IList<string> GetContractRecipients(ContractDTO contract, string dealerEmail)
        {
            var recipients = new List<string>();

            if (contract.PrimaryCustomer?.Emails?.Any() ?? false)
            {
                recipients.Add(contract.PrimaryCustomer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                    contract.PrimaryCustomer.Emails.First().EmailAddress);
            }

            if (contract?.SecondaryCustomers?.Any() ?? false)
            {
                contract.SecondaryCustomers.ForEach(c =>
                {
                    if (c.Emails?.Any() ?? false)
                    {
                        recipients.Add(c.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                            c.Emails.First().EmailAddress);
                    }
                });
            }

            //TODO: dealer and ODI/Ecohome team
            if (!string.IsNullOrEmpty(dealerEmail))
            {
                recipients.Add(dealerEmail);
            }

            return recipients;
        }

        private async Task<IList<Alert>> SendEmail(IList<string> recipients, string subject, string body)
        {
            var alerts = new List<Alert>();
            HttpClient client = new HttpClient();
            string baseUri = string.Empty;
            string domain = string.Empty;
            string key = string.Empty;
            string sender = string.Empty;

            try
            {
                baseUri = ConfigurationManager.AppSettings["MailGun.ApiUrl"];
                domain = ConfigurationManager.AppSettings["MailGun.Domain"];
                key = ConfigurationManager.AppSettings["MailGun.ApiKey"];
                sender = ConfigurationManager.AppSettings["MailGun.From"];
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't get mailgun settings from config";
                _loggingService.LogError(errorMsg, ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = errorMsg,
                    Message = ex.ToString()
                });
            }            

            client.BaseAddress = new Uri(baseUri);            
            var credentials = Encoding.ASCII.GetBytes($"api:{key}");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));

            var requestValues = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("domain", domain),
                new KeyValuePair<string, string>("from", sender),
                new KeyValuePair<string, string>("subject", subject),
                new KeyValuePair<string, string>("text", body)
            };

            recipients?.ForEach(r =>
                requestValues.Add(new KeyValuePair<string, string>("to", r)));

            var data = new FormUrlEncodedContent(requestValues);
            
            var requestUri = new Uri(new Uri(baseUri), $"{domain}/messages");

            var response = await client.PostAsync(requestUri, data);
            response.EnsureSuccessStatusCode();            

            return alerts;
        }
    }
}
