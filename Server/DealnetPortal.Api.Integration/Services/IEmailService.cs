using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IEmailService : IIdentityMessageService
    {
        Task SendAsync(MailMessage message);
    }
}
