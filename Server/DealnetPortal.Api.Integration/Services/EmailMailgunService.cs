﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Messaging;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public class EmailMailgunService : IEmailService
    {
        public async Task SendAsync(IList<string> recipients, string from, string subject, string body)
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
                sender = from ?? ConfigurationManager.AppSettings["MailGun.From"];
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't get mailgun settings from config";
                //_loggingService.LogError(errorMsg, ex);
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

            //return alerts;
        }

        public Task SendAsync(MailMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
