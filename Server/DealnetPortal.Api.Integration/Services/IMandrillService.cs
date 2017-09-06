using System.Net.Http;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Notification;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMandrillService
    {
        Task SendDealerLeadAccepted(Contract contract, DealerDTO dealer, string services);
        Task<HttpResponseMessage> SendEmail(MandrillRequest request);
        Task SendHomeImprovementTypeUpdatedConfirmation(string emailid, string firstName, string lastName, string services);
        Task SendDeclineNotificationConfirmation(string emailid, string firstName, string lastName);
        Task SendProblemsWithSubmittingOnboarding(string errorMsg, int dealerInfoId, string accessKey);
        Task SendDraftLinkMail(string accessKey, string email);
    }
}