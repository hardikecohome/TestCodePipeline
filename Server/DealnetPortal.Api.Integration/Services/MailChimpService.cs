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

        public async Task<Queue> SendUpdateNotification(string email)
        {
            try
            {
                Queue q = await manager.AutomationEmailQueues.AddSubscriberAsync("154521", "154525", email);
                return q;
            }
            catch (MailChimpException me)
            {
                throw me;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public static MailChimpManager manager = new MailChimpManager();

        //public async Task AddNewSubscriberAsync(RegisterCustomerBindingModel customerData)
        //{
        //    string listId = "dc1b598897";
        //    Member subscriber = await manager.Members.GetAsync(listId, customerData.Profile.EmailAddress);

        //    if (subscriber != null)
        //    {
        //        if (customerData.Profile.FirstName != null)
        //        {
        //            subscriber.MergeFields["FNAME"] = customerData.Profile.FirstName;
        //        }
        //        if (customerData.Profile.LastName != null)
        //        {
        //            subscriber.MergeFields["LNAME"] = customerData.Profile.LastName;
        //        }

        //        var location = customerData.Profile.Locations.FirstOrDefault();
        //        if (location != null)
        //        {
        //            subscriber.MergeFields["ADDRESS"] = new
        //            {
        //                addr1 = location.Street,
        //                addr2 = location.Unit,
        //                city = location.City,
        //                state = location.State,
        //                zip = location.PostalCode,
        //                country = "CA"
        //            };
        //        }
        //        if (customerData.TransactionInfo.CreditAmount != null)
        //        {
        //            subscriber.MergeFields["CREDITAMT"] = customerData.TransactionInfo.CreditAmount;
        //        }
        //        if (customerData.TransactionInfo.AspireStatus != null)
        //        {
        //            subscriber.MergeFields["APPSTATUS"] = customerData.TransactionInfo.AspireStatus;
        //        }
        //        if (customerData.TransactionInfo.IsIncomplete == true)
        //        {
        //            subscriber.MergeFields["EQUIPINFO"] = "Required";
        //        }
        //        else
        //        {
        //            subscriber.MergeFields["EQUIPINFO"] = "Not Required";
        //        }
        //        subscriber.MergeFields["TPASSWORD"] = customerData.RegisterInfo.Password;
        //    }
        //    else
        //    {
        //        subscriber.EmailAddress = customerData.Profile.EmailAddress;
        //        subscriber.MergeFields["FNAME"] = customerData.Profile.FirstName;
        //        subscriber.MergeFields["LNAME"] = customerData.Profile.LastName;
        //        subscriber.EmailType = "HTML";
        //        subscriber.StatusIfNew = Status.Pending;
        //        subscriber.ListId = listId;
        //        var location = customerData.Profile.Locations.FirstOrDefault();
        //        subscriber.MergeFields["ADDRESS"] = new
        //        {
        //            addr1 = location.Street,
        //            addr2 = location.Unit,
        //            city = location.City,
        //            state = location.State,
        //            zip = location.PostalCode,
        //            country = "CA"
        //        };
        //        subscriber.MergeFields["CREDITAMT"] = customerData.TransactionInfo.CreditAmount;
        //        subscriber.MergeFields["APPSTATUS"] = customerData.TransactionInfo.AspireStatus;
        //        if (customerData.TransactionInfo.IsIncomplete == true)
        //        {
        //            subscriber.MergeFields["EQUIPINFO"] = "Required";
        //        }
        //        else
        //        {
        //            subscriber.MergeFields["EQUIPINFO"] = "Not Required";
        //        }
        //        if (customerData.RegisterInfo.Password != null)
        //        {
        //            subscriber.MergeFields["TPASSWORD"] = customerData.RegisterInfo.Password;
        //        }
        //    }
        //    try
        //    {
        //        var result = await manager.Members.AddOrUpdateAsync(listId, subscriber);
        //    }
        //    catch (MailChimpException mce)
        //    {
        //        throw mce;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
    }
}
