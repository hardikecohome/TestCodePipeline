using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IEmailService
    {
        Task SendAsync(IList<string> recipients, string from, string subject, string body);

        Task SendAsync(MailMessage message);
    }
}
