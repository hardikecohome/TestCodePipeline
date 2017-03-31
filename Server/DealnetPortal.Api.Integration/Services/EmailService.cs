using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Utilities;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public class EmailService: IEmailService, IIdentityMessageService
    {        
        public async Task SendAsync(IList<string> recipients, string from, string subject, string body)
        {
            var message = new IdentityMessage()
            {
                Body = body,
                Subject = Resources.Resources.NewCustomerAppliedForFinancing,
                Destination = recipients?.FirstOrDefault() ?? string.Empty
            };
            try
            {
                await SendAsync(message);
            }
            catch (Exception ex)
            {                
                throw ex;
            }            
        }

        public async Task SendAsync(MailMessage message)
        {
            using (var smtpClient = new SmtpClient(ConfigurationManager.AppSettings["EmailService.SmtpHost"],
                    Convert.ToInt32(ConfigurationManager.AppSettings["EmailService.SmtpPort"])))
            {
                var credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailService.SmtpUser"], ConfigurationManager.AppSettings["EmailService.SmtpPassword"]);
                smtpClient.Credentials = credentials;
                try
                {
                    await smtpClient.SendMailAsync(message);
                }
                catch (Exception ex)
                {                    
                    throw ex;
                }                
            }
        }

        #region IIdentityMessageService implementation
        public async Task SendAsync(IdentityMessage message)
        {
            var text = message.Body;
            var html = message.Body;
            var msg = new MailMessage
            {
                From = new MailAddress(ConfigurationManager.AppSettings["EmailService.FromEmailAddress"]),
                Subject = message.Subject
            };
            msg.To.Add(new MailAddress(message.Destination));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(text, null, MediaTypeNames.Text.Plain));
            msg.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(html, null, MediaTypeNames.Text.Html));

            var smtpClient = new SmtpClient(ConfigurationManager.AppSettings["EmailService.SmtpHost"], Convert.ToInt32(ConfigurationManager.AppSettings["EmailService.SmtpPort"]));
            var credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailService.SmtpUser"], ConfigurationManager.AppSettings["EmailService.SmtpPassword"]);
            using (smtpClient)
            {
                smtpClient.Credentials = credentials;
                try
                {
                    await smtpClient.SendMailAsync(msg);
                }
                catch (Exception ex)
                {
                    
                    throw ex;
                }                
            }
        }        
        #endregion
    }
}