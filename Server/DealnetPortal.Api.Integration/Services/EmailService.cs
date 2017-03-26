using System;
using System.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public class EmailService: IEmailService
    {
        public Task SendAsync(IdentityMessage message)
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
            smtpClient.Credentials = credentials;
            smtpClient.Send(msg);

            return Task.FromResult(0);
        }

        public async Task SendAsync(MailMessage message)
        {
            using (var smtpClient = new SmtpClient(ConfigurationManager.AppSettings["EmailService.SmtpHost"],
                    Convert.ToInt32(ConfigurationManager.AppSettings["EmailService.SmtpPort"])))
            {
                var credentials = new System.Net.NetworkCredential(ConfigurationManager.AppSettings["EmailService.SmtpUser"], ConfigurationManager.AppSettings["EmailService.SmtpPassword"]);
                smtpClient.Credentials = credentials;
                await smtpClient.SendMailAsync(message);
            }
        }
    }
}