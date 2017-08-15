using System;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.CustomerWallet;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Models;
using DealnetPortal.Api.Models.Notification;
namespace DealnetPortal.Api.Integration.Services
{
    public class MailChimpService : IMailСhimpService
    {
        public static MailChimpManager manager;
        public MailChimpService(string apiKey)
        {
            manager = new MailChimpManager(apiKey);
        }

        public async Task AddNewSubscriberAsync(string listid, MailChimpMember member)
        {
            Member subscriber = new Member();
            subscriber.EmailAddress = member.Email;
            subscriber.MergeFields["FNAME"] = member.FirstName;
            subscriber.MergeFields["LNAME"] = member.LastName;
            subscriber.EmailType = "HTML";
            subscriber.StatusIfNew = Status.Pending;
            subscriber.ListId = listid;
            subscriber.MergeFields["ADDRESS"] = new
            {
                addr1 = member.address.Street,
                addr2 = member.address.Unit,
                city = member.address.City,
                state = member.address.State,
                zip = member.address.PostalCode,
                country = member.address.Country
            };
            subscriber.MergeFields["CREDITAMT"] = member.CreditAmount;
            subscriber.MergeFields["APPSTATUS"] = member.ApplicationStatus;
            subscriber.MergeFields["EQUIPINFO"] = member.EquipmentInfoRequired;
            subscriber.MergeFields["TPASSWORD"] = member.TemporaryPassword;
            subscriber.MergeFields["CLOSINGREQ"] = member.ClosingDateRequired;
            subscriber.MergeFields["CWLINK"] = member.OneTimeLink;
            subscriber.MergeFields["DEALERLEAD"] = member.DealerLeadAccepted;
            try
            {

                await manager.Members.AddOrUpdateAsync(listid, subscriber);
            }
            catch (MailChimpException mce)
            {
                throw mce;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> isSubscriber(string listid, string emailid)
        {
            Member subscriber = new Member();
            subscriber = await manager.Members.GetAsync(listid, emailid.ToLower());
            if (subscriber.Status == Status.Subscribed)
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
}
