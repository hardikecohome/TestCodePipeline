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
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class MailService : IMailService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;

        public MailService(IContractRepository contractRepository, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;


        }

        public async Task<IList<Alert>> SendSubmitNotification(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                var subject = $"Contract {contract.Id} was successfully submitted";
                var body = new StringBuilder();
                body.AppendLine($"Contract {contract.Id} was successfully submitted");
                body.AppendLine($"Type of Application: {contract.Equipment.AgreementType}");
                body.AppendLine($"Home Owner's Name: {contract.PrimaryCustomer?.FirstName} {contract.PrimaryCustomer?.LastName}");
                var recipients = new List<string>();

                try
                {
                    var sAlerts = await SendEmail(recipients, subject, body.ToString());
                    if (sAlerts?.Any() ?? false)
                    {
                        alerts.AddRange(sAlerts);
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
                alerts.Add(new Alert()
                {
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contractId}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contractId}");
            }

            return alerts;
        }

        private async Task<IList<Alert>> SendEmail(IList<string> recipients, string subject, string body)
        {
            HttpClient client = new HttpClient();
            string baseUri = ConfigurationManager.AppSettings["EmailService.FromEmailAddress"];
            client.BaseAddress = new Uri(baseUri);
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("api", "key-7b6273c4442da6aaa9496eb3eed25036");
            var credentials = Encoding.ASCII.GetBytes("api:key-7b6273c4442da6aaa9496eb3eed25036");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(credentials));


            var domain = "sandbox36ed7e337cd34757869b6c132e07e7b0.mailgun.org";

            var data = new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("domain", domain),
                new KeyValuePair<string, string>("from", "Mailgun Sandbox <postmaster@sandbox36ed7e337cd34757869b6c132e07e7b0.mailgun.org>"),
                new KeyValuePair<string, string>("to", "mkhar@yandex.ru"),
                new KeyValuePair<string, string>("subject", "Hello Maksim"),
                new KeyValuePair<string, string>("text", "Congratulations Maksim, you just sent an email with Mailgun!  You are truly awesome!  You can see a record of this email in your logs: https://mailgun.com/cp/log .  You can send up to 300 emails/day from this sandbox server.  Next, you should add your own domain so you can send 10,000 emails/month for free."),
            });

            var requestUri = new Uri(new Uri(baseUri), $"{domain}/messages");

            var response = client.PostAsync(requestUri, data).GetAwaiter().GetResult();
        }
    }
}
