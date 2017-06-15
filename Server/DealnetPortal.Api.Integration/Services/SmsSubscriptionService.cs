using System;
using System.Threading.Tasks;
using DealnetPortal.Api.Integration.SMSSubscriptionManagement;

namespace DealnetPortal.Api.Integration.Services
{
    public class SmsSubscriptionService : ISmsSubscriptionService
    {
        private readonly subscriptionManagementAPI _smsClient = new subscriptionManagementAPIClient();
        private readonly startSubscriptionDTO _subscriber = new startSubscriptionDTO();

        public async Task<startSubscriptionResponse> SetStartSubscription(string phone, string reference, string code)
        {
            try
            {
                contentServiceIdDTO content = new contentServiceIdDTO();
                content.reference = System.Configuration.ConfigurationManager.AppSettings["SubscriptionRef"];

                _subscriber.phone = phone;
                _subscriber.reference = reference;
                _subscriber.contentService = content;
                _subscriber.affiliateCode = code;

                startSubscriptionResponse response = await _smsClient.startSubscriptionAsync(new startSubscription(_subscriber));

                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
