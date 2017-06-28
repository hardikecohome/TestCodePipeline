using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using DealnetPortal.Api.Models.Notification;
using DealnetPortal.Domain;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public class MandrillService : IMandrillService
    {
        //public IMandrillApi _manager;
        public static string _endPoint { get; set; }
        public static string _apiKey { get; set; }

        public MandrillService()
        {
            _endPoint = ConfigurationManager.AppSettings["MandrillEndPoint"];
            _apiKey = ConfigurationManager.AppSettings["MandrillApiKey"];
        }
        public async Task<HttpResponseMessage> SendEmail(MandrillRequest request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ConfigurationManager.AppSettings["MandrillEndPoint"]);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                try
                {
                    
                    return await client.PostAsJsonAsync("/api/1.0/messages/send-template.json", request);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
        public async Task SendDealerLeadAccepted(Contract contract, DealerDTO dealer, string services)
        {
            string emailid = contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress;
            var location = dealer.Locations?.FirstOrDefault();
            var addres = location != null ? $"{location.Street}, {location.City}, {location.State}, {location.PostalCode}" : "";

            MandrillRequest request = new MandrillRequest();
            List<Variable> myVariables = new List<Variable>();
            myVariables.Add(new Variable() { name = "FNAME", content = contract.PrimaryCustomer.FirstName });
            myVariables.Add(new Variable() { name = "LNAME", content = contract.PrimaryCustomer.LastName });
            myVariables.Add(new Variable() { name = "EQUIPINFO", content = services });
            //myVariables.Add(new Variable() { name = "DADDRESS", content = addres});
            //myVariables.Add(new Variable() { name = "DPHONE", content = dealer.Phones.First().PhoneNum != null? dealer.Phones.First().PhoneNum: "" });
            //myVariables.Add(new Variable() { name = "DMAIL", content = dealer.Emails.FirstOrDefault().EmailAddress != null? dealer.Emails.FirstOrDefault().EmailAddress :"" });
            request.key = _apiKey;
            request.template_name = ConfigurationManager.AppSettings["DealerLeadAcceptedTemplate"];
            request.template_content = new List<templatecontent>() {
                new templatecontent(){
                    name="Dealer Accepted Lead",
                    content = "Dealer Accepted Your Lead"
                }
            };


            request.message = new MandrillMessage()
            {
                from_email = ConfigurationManager.AppSettings["FromEmail"],
                from_name = "Eco Home Financial",
                html = null,
                merge_vars = new List<MergeVariable>() {
                    new MergeVariable(){
                        rcpt = emailid,
                        vars = myVariables
                             
                        
                    }
                },
                send_at = DateTime.Now,
                subject = "Dealer accepted your lead.",
                text = "Dealer accepted your lead.",
                to = new List<MandrillTo>() {
                    new MandrillTo(){
                        email =emailid,
                        name = contract.PrimaryCustomer.FirstName+" "+contract.PrimaryCustomer.LastName,
                        type = "to"
                    }
                }
            };
            try
            {
                var result = await SendEmail(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task SendHomeImprovementTypeUpdatedConfirmation(string emailid, string firstName, string lastName, string services)
        {
            MandrillRequest request = new MandrillRequest();
            List<Variable> myVariables = new List<Variable>();
            myVariables.Add(new Variable() { name = "FNAME", content = firstName });
            myVariables.Add(new Variable() { name = "LNAME", content = lastName });
            myVariables.Add(new Variable() { name = "EQUIPINFO", content = services });
            request.key = _apiKey;
            request.template_name = ConfigurationManager.AppSettings["HomeImprovementTypeUpdatedTemplate"];
            request.template_content = new List<templatecontent>() {
                    new templatecontent(){
                        name="We’re looking for the best professional for your home improvement project",
                        content = "We’re looking for the best professional for your home improvement project"
                    }
                };


            request.message = new MandrillMessage()
            {
                from_email = ConfigurationManager.AppSettings["FromEmail"],
                from_name = "Eco Home Financial",
                html = null,
                merge_vars = new List<MergeVariable>() {
                        new MergeVariable(){
                            rcpt = emailid,
                            vars = myVariables


                        }
                    },
                send_at = DateTime.Now,
                subject = "We’re looking for the best professional for your home improvement project",
                text = "We’re looking for the best professional for your home improvement project",
                to = new List<MandrillTo>() {
                        new MandrillTo(){
                            email =emailid,
                            name = firstName+" "+lastName,
                            type = "to"
                        }
                    }
            };
            try
            {
                var result = await SendEmail(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task SendDeclineNotificationConfirmation(string emailid, string firstName, string lastName)
        {
            MandrillRequest request = new MandrillRequest();
            List<Variable> myVariables = new List<Variable>();
            myVariables.Add(new Variable() { name = "FNAME", content = firstName });
            myVariables.Add(new Variable() { name = "LNAME", content = lastName });
            request.key = _apiKey;
            request.template_name = ConfigurationManager.AppSettings["DeclinedOrCreditReviewTemplate"];
            request.template_content = new List<templatecontent>() {
                    new templatecontent(){
                        name="Declined",
                        content = "Declined"
                    }
                };


            request.message = new MandrillMessage()
            {
                from_email = ConfigurationManager.AppSettings["FromEmail"],
                from_name = "Eco Home Financial",
                html = null,
                merge_vars = new List<MergeVariable>() {
                        new MergeVariable(){
                            rcpt = emailid,
                            vars = myVariables


                        }
                    },
                send_at = DateTime.Now,
                subject = "Application Declined or Credit review.",
                text = "Application Declined or Credit review.",
                to = new List<MandrillTo>() {
                        new MandrillTo(){
                            email =emailid,
                            name = firstName+" "+lastName,
                            type = "to"
                        }
                    }
            };
            try
            {
                var result = await SendEmail(request);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        //    public async Task SendEmail(string myemail)
        //{
        //    List<EmailAddress> to = new List<EmailAddress>();
        //    List<RcptMergeVar> mergevariables = new List<RcptMergeVar>();
        //    List<MergeVar> myvar = new List<MergeVar>();
        //    myvar.Add(new MergeVar() { Name="FNAME", Content = "Hardik" });
        //    myvar.Add(new MergeVar() { Name = "LNAME", Content = "Sharma" });
        //    List<TemplateContent> variables = new List<TemplateContent>();

        //    mergevariables.Add(new RcptMergeVar() {
        //        Rcpt = myemail,
        //        Vars = myvar

        //    });
        //    to.Add(new EmailAddress() { Email = myemail });
        //    EmailMessage message = new EmailMessage();
        //    message.To = to;

        //    await _manager.SendMessageTemplate(new SendMessageTemplateRequest(new EmailMessage()
        //    {
        //        To = to,
        //        Subject = "Send Email From Mandrill",
        //        FromEmail = ConfigurationManager.AppSettings["FromEamil"],
        //        FromName = ConfigurationManager.AppSettings["FromName"],
        //        Merge = true,
        //        MergeLanguage = "mailchimp"

        //    }, "",));
        //}

    }
}
