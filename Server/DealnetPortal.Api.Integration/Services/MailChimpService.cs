using System;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.CustomerWallet;
using MailChimp.Net;
using MailChimp.Net.Core;
using MailChimp.Net.Models;

namespace DealnetPortal.Api.Integration.Services
{
    public class MailChimpService : IMailСhimpService
    {
        public static MailChimpManager manager = new MailChimpManager();

        public async Task AddNewSubscriberAsync(RegisterCustomerBindingModel customerData)
        {
            string listId = "dc1b598897";
            Member subscriber = await manager.Members.GetAsync(listId, customerData.Profile.EmailAddress);

            var transactionInfo = customerData.TransactionsInfo.FirstOrDefault();
            if (subscriber != null)
            {
                if (customerData.Profile.FirstName != null)
                {
                    subscriber.MergeFields["FNAME"] = customerData.Profile.FirstName;
                }
                if (customerData.Profile.LastName != null)
                {
                    subscriber.MergeFields["LNAME"] = customerData.Profile.LastName;
                }

                var location = customerData.Profile.Locations.FirstOrDefault();
                if (location != null)
                {
                    subscriber.MergeFields["ADDRESS"] = new
                    {
                        addr1 = location.Street,
                        addr2 = location.Unit,
                        city = location.City,
                        state = location.State,
                        zip = location.PostalCode,
                        country = "CA"
                    };
                }
                if (transactionInfo?.CreditAmount != null)
                {
                    subscriber.MergeFields["CREDITAMT"] = transactionInfo.CreditAmount;
                }
                if (transactionInfo?.AspireStatus != null)
                {
                    subscriber.MergeFields["APPSTATUS"] = transactionInfo.AspireStatus;
                }
                if (transactionInfo?.IsIncomplete == true)
                {
                    subscriber.MergeFields["EQUIPINFO"] = "Required";
                }
                else
                {
                    subscriber.MergeFields["EQUIPINFO"] = "Not Required";
                }
                subscriber.MergeFields["TPASSWORD"] = customerData.RegisterInfo.Password;
            }
            else
            {
                subscriber.EmailAddress = customerData.Profile.EmailAddress;
                subscriber.MergeFields["FNAME"] = customerData.Profile.FirstName;
                subscriber.MergeFields["LNAME"] = customerData.Profile.LastName;
                subscriber.EmailType = "HTML";
                subscriber.StatusIfNew = Status.Pending;
                subscriber.ListId = listId;
                var location = customerData.Profile.Locations.FirstOrDefault();
                subscriber.MergeFields["ADDRESS"] = new
                {
                    addr1 = location.Street,
                    addr2 = location.Unit,
                    city = location.City,
                    state = location.State,
                    zip = location.PostalCode,
                    country = "CA"
                };
                subscriber.MergeFields["CREDITAMT"] = transactionInfo?.CreditAmount;
                subscriber.MergeFields["APPSTATUS"] = transactionInfo?.AspireStatus;
                if (transactionInfo?.IsIncomplete == true)
                {
                    subscriber.MergeFields["EQUIPINFO"] = "Required";
                }
                else
                {
                    subscriber.MergeFields["EQUIPINFO"] = "Not Required";
                }
                if (customerData.RegisterInfo.Password != null)
                {
                    subscriber.MergeFields["TPASSWORD"] = customerData.RegisterInfo.Password;
                }
            }
            try
            {
                var result = await manager.Members.AddOrUpdateAsync(listId, subscriber);
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
    }
}
