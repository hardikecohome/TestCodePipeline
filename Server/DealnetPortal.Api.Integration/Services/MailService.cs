using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Routing;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Utilities.Messaging;
using DealnetPortal.Api.Models.Notification;

namespace DealnetPortal.Api.Integration.Services
{
    public class MailService : IMailService
    {
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly ISmsSubscriptionService _smsSubscriptionServive = new SmsSubscriptionService();
        private readonly IPersonalizedMessageService _personalizedMessageService = new PersonalizedMessageService(ConfigurationManager.AppSettings["SMSENDPOINT"],
                                                                                                                     ConfigurationManager.AppSettings["SMSAPIKEY"]);
        private readonly IMailСhimpService _mailChimpService = new MailChimpService(ConfigurationManager.AppSettings["MailChimpApiKey"]);
        private readonly IMandrillService _mandrillService = new MandrillService();

        public MailService(IEmailService emailService, IContractRepository contractRepository, ILoggingService loggingService)
        {
            _emailService = emailService;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
        }

        #region DP
        public async Task<IList<Alert>> SendContractSubmitNotification(ContractDTO contract, string dealerEmail, bool success = true)
        {
            var alerts = new List<Alert>();
            var id = contract.Details?.TransactionId ?? contract.Id.ToString();
            var subject = string.Format(success ? Resources.Resources.ContractWasSuccessfullySubmitted : Resources.Resources.ContractWasDeclined, id);
            var body = new StringBuilder();
            body.AppendLine(subject);
            body.AppendLine($"{Resources.Resources.TypeOfApplication} {contract.Equipment.AgreementType.GetEnumDescription()}");
            body.AppendLine($"{Resources.Resources.HomeOwnersName} {contract.PrimaryCustomer?.FirstName} {contract.PrimaryCustomer?.LastName}");
            await SendNotification(body.ToString(), subject, contract, dealerEmail, alerts);                

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }

            return alerts;
        }

        public async Task<IList<Alert>> SendContractChangeNotification(ContractDTO contract, string dealerEmail)
        {
            var alerts = new List<Alert>();
            
            var id = contract.Details?.TransactionId ?? contract.Id.ToString();
            var subject = string.Format(Resources.Resources.ContractWasSuccessfullyChanged, id);
            var body = new StringBuilder();
            body.AppendLine(subject);
            body.AppendLine($"{Resources.Resources.TypeOfApplication} {contract.Equipment.AgreementType.GetEnumDescription()}");
            body.AppendLine(
                $"{Resources.Resources.HomeOwnersName} {contract.PrimaryCustomer?.FirstName} {contract.PrimaryCustomer?.LastName}");
            await SendNotification(body.ToString(), subject, contract, dealerEmail, alerts);

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }

            return alerts;
        }

        public async Task SendDealerLoanFormContractCreationNotification(CustomerFormDTO customerFormData, CustomerContractInfoDTO contractData)
        {
            var address = string.Empty;
            var addresItem = customerFormData.PrimaryCustomer.Locations.FirstOrDefault(ad => ad.AddressType == AddressType.MainAddress);

            if (addresItem != null)
            {
                address = $"{addresItem.Street}, {addresItem.City}, {addresItem.State}, {addresItem.PostalCode}";
            }
            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.NewCustomerAppliedForFinancing}</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<p>{Resources.Resources.ContractId}: {contractData.ContractId}</p>");
            body.AppendLine($"<p><b>{Resources.Resources.Name}: {$"{customerFormData.PrimaryCustomer.FirstName} {customerFormData.PrimaryCustomer.LastName}"}</b></p>");
            if (contractData.CreditAmount > 0)
            {
                body.AppendLine($"<p><b>{Resources.Resources.PreApproved}: ${contractData.CreditAmount.ToString("N0", CultureInfo.InvariantCulture)}</b></p>");
            }
            body.AppendLine($"<p><b>{Resources.Resources.SelectedTypeOfService}: {customerFormData.SelectedService ?? string.Empty}</b></p>");
            body.AppendLine($"<p>{Resources.Resources.Comment}: {customerFormData.CustomerComment}</p>");
            body.AppendLine($"<p>{Resources.Resources.InstallationAddress}: {address}</p>");
            body.AppendLine($"<p>{Resources.Resources.HomePhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.CellPhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.BusinessPhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.Email}: {customerFormData.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.YouCanViewThisDealHere}: <a href=\"{customerFormData.DealUri}/{contractData.ContractId}\">{Resources.Resources.DealInfo}</a></p>");
            body.AppendLine("</div>");

            //var alternateView = AlternateView.CreateAlternateViewFromString(body.ToString(), null, MediaTypeNames.Text.Html);

            //var mail = new MailMessage {IsBodyHtml = true};

            //mail.AlternateViews.Add(alternateView);
            //mail.From = new MailAddress(ConfigurationManager.AppSettings["EmailService.FromEmailAddress"]);
            //mail.To.Add(contractData.DealerEmail);
            //mail.Subject = Resources.Resources.ThankYouForApplyingForFinancing;

            try
            {
                //await _emailService.SendAsync(mail);
                await _emailService.SendAsync(new List<string> { contractData.DealerEmail ?? string.Empty }, string.Empty, Resources.Resources.ThankYouForApplyingForFinancing, body.ToString());
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }            
        }

        public async Task SendCustomerLoanFormContractCreationNotification(string customerEmail, CustomerContractInfoDTO contractData, string dealerColor, byte[] dealerLogo)
        {
            var location = contractData.DealerAdress;
            var html = File.ReadAllText(HostingEnvironment.MapPath(@"~\Content\emails\customer-notification-email.html"));
            var bodyBuilder = new StringBuilder(html, html.Length * 2);
            bodyBuilder.Replace("{headerColor}", dealerColor ?? "#2FAE00");
            bodyBuilder.Replace("{thankYouForApplying}", Resources.Resources.ThankYouForApplyingForFinancing);
            bodyBuilder.Replace("{youHaveBeenPreapprovedFor}", contractData.CreditAmount != 0 ? Resources.Resources.YouHaveBeenPreapprovedFor.Replace("{0}", contractData.CreditAmount.ToString("N0", CultureInfo.InvariantCulture)) : string.Empty);
            bodyBuilder.Replace("{yourApplicationWasSubmitted}", Resources.Resources.YourFinancingApplicationWasSubmitted);
            bodyBuilder.Replace("{willContactYouSoon}", Resources.Resources.WillContactYouSoon.Replace("{0}", contractData.DealerName ?? Resources.Resources.Dealer));
            LinkedResource inlineLogo = null;
            var inlineSuccess = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-success.png"));
            inlineSuccess.ContentId = Guid.NewGuid().ToString();
            inlineSuccess.ContentType.MediaType = "image/png";
            bodyBuilder.Replace("{successIcon}", "cid:" + inlineSuccess.ContentId);

            var body = bodyBuilder.ToString();
            if (contractData.DealerEmail == null && contractData.DealerPhone == null &&
                contractData.DealerAdress == null && contractData.DealerName == null)
            {
                var contactSectionPattern = @"{ContactSectionStart}(.*?){ContactSectionEnd}";
                body = Regex.Replace(body, contactSectionPattern, "", RegexOptions.Singleline);
            }
            else
            {
                var contactSectionTagsPattern = @"{ContactSection(.*?)}";
                body = Regex.Replace(body, contactSectionTagsPattern, "", RegexOptions.Singleline);
                body = body.Replace("{ifYouHavePleaseContact}", Resources.Resources.IfYouHaveQuestionsPleaseContact);
                body = body.Replace("{dealerName}", contractData.DealerName ?? "");
                body = body.Replace("{dealerAddress}", location != null ? $"{location?.Street}, {location?.City}, {location?.State}, {location?.PostalCode}" : "");
                if (contractData.DealerPhone == null)
                {
                    var phoneSectionPattern = @"{PhoneSectionStart}(.*?){PhoneSectionEnd}";
                    body = Regex.Replace(body, phoneSectionPattern, "", RegexOptions.Singleline);
                }
                else
                {
                    var phoneSectionTagsPattern = @"{PhoneSection(.*?)}";
                    body = Regex.Replace(body, phoneSectionTagsPattern, "", RegexOptions.Singleline);
                    body = body.Replace("{phone}", Resources.Resources.Phone);
                    body = body.Replace("{dealerPhone}", contractData.DealerPhone);
                }
                if (contractData.DealerEmail == null)
                {
                    var mailSectionPattern = @"{MailSectionStart}(.*?){MailSectionEnd}";
                    body = Regex.Replace(body, mailSectionPattern, "", RegexOptions.Singleline);
                }
                else
                {
                    var mailSectionTagsPattern = @"{MailSection(.*?)}";
                    body = Regex.Replace(body, mailSectionTagsPattern, "", RegexOptions.Singleline);
                    body = body.Replace("{mail}", Resources.Resources.Email);
                    body = body.Replace("{dealerMail}", contractData.DealerEmail);
                }
            }
            
            if (dealerLogo == null)
            {
                var logoPattern = @"{LogoStart}(.*?){LogoEnd}";
                body = Regex.Replace(body, logoPattern, "", RegexOptions.Singleline);
            }
            else
            {
                var logoTagsPattern = @"{Logo(.*?)}";
                body = Regex.Replace(body, logoTagsPattern, "", RegexOptions.Singleline);
                inlineLogo = new LinkedResource(new MemoryStream(dealerLogo));
                inlineLogo.ContentId = Guid.NewGuid().ToString();
                inlineLogo.ContentType.MediaType = "image/png";
                body = body.Replace("{dealerLogo}", "cid:" + inlineLogo.ContentId);
            }
            var alternateView = AlternateView.CreateAlternateViewFromString(body, null,
                    MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(inlineSuccess);
            if (inlineLogo != null)
            {
                alternateView.LinkedResources.Add(inlineLogo);
            }

            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.AlternateViews.Add(alternateView);
            mail.From = new MailAddress(contractData.DealerEmail);
            mail.To.Add(customerEmail);
            mail.Subject = Resources.Resources.ThankYouForApplyingForFinancing;
            try
            {
                await _emailService.SendAsync(mail);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }
        #endregion

        #region Public MB
        public async Task SendInviteLinkToCustomer(Contract customerFormData, string password)
        {
            string customerEmail = customerFormData.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string domain = ConfigurationManager.AppSettings["CustomerWalletClient"];
            string hashLogin = SecurityUtils.Hash(customerEmail);
            string mbPhone = ConfigurationManager.AppSettings["CustomerWalletPhone"];
            string mbEmail = ConfigurationManager.AppSettings["CustomerWalletEmail"];

            var phoneIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-phone.png"));
            var phoneImage = GenerateIconImageCid(phoneIcon);
            var emailIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-email.png"));
            var emailImage = GenerateIconImageCid(emailIcon);

            var bottomStyle = "style='font-size: 10px; !important'";
            var pStyle = "style='font-size: 18px; !important'";

            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.Hi} {customerFormData.PrimaryCustomer.FirstName},</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.Congratulations}, {Resources.Resources.YouHaveBeen} <b>{Resources.Resources.PreApproved.ToLower()} {Resources.Resources.For} ${customerFormData.Details.CreditAmount.Value.ToString("N0", CultureInfo.InvariantCulture)}</b>.</p>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.YouCanViewYourAccountOn} <b><a href='{domain}/invite/{hashLogin}'><span>{domain}</span></a></b></p>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.PleaseSignInUsingYourEmailAddressAndFollowingPassword}: {password}</p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p>{Resources.Resources.InCaseOfQuestionsPleaseContact} <b>EcoHome Financial</b>  {Resources.Resources.Support.ToLower()}:</p>");
            body.AppendLine($"<p><img src='{phoneImage}'>{mbPhone}</p>");
            body.AppendLine($"<p><img src='{emailImage}'/> <a href='mailto:{mbEmail}'><span>{mbEmail}</span></a></li></p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p {bottomStyle}><b>This email was sent by EcoHome Financial</b> | 325 Milner Avenue, Suite 300 | Toronto, Ontario | M1B 5N1 Canada</p>");
            body.AppendLine($"<p {bottomStyle}><b>Contact us:</b> {mbPhone} | {mbEmail}</p>");
            body.AppendLine($"<p {bottomStyle}>We truly hope you found this message useful. However, if you'd rather not receive future e-mails of this sort from EcoHome Financial, please <b><a href='{domain}/unsubscribe/{hashLogin}'><span>click here to unsubscribe.</span></a></b>.</p>");

            body.AppendLine("</div>");

            var alternateView = GenerateAlternateView(body, new List<LinkedResource>() { phoneIcon, emailIcon });

            var subject = $"{Resources.Resources.Congratulations}, {Resources.Resources.YouHaveBeen} {Resources.Resources.PreApproved.ToLower()} {Resources.Resources.For} ${customerFormData.Details.CreditAmount.Value.ToString("N0", CultureInfo.InvariantCulture)}";
            var mail = GenerateMailMessage(customerEmail, subject, alternateView);
            MailChimpMember member = new MailChimpMember()
            {
                Email = customerFormData.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                FirstName = customerFormData.PrimaryCustomer.FirstName,
                LastName = customerFormData.PrimaryCustomer.LastName,
                address = new MemberAddress()
                {
                    Street = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().Street,
                    Unit = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().Unit,
                    City = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().City,
                    State = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().State,
                    PostalCode = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().PostalCode
                },
                CreditAmount = (decimal)customerFormData.Details.CreditAmount,
                ApplicationStatus = customerFormData.ContractState.ToString(),
                TemporaryPassword = password,
                EquipmentInfoRequired = "Required",
                OneTimeLink = domain +"/invite/"+ hashLogin 
                //EquipmentInfoRequired = (customerFormData.Equipment.NewEquipment.FirstOrDefault().Type == null) ? "Required" : "Not Required"
            };

            try
            {
              //  await _emailService.SendAsync(mail);
              // Hardik SMS trigger for subscription request
                var result = await _smsSubscriptionServive.setstartsubscription(customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell).PhoneNum,
                                                                                customerFormData.PrimaryCustomer.Id.ToString(),
                                                                              "Broker",
                                                                            ConfigurationManager.AppSettings["SubscriptionRef"]);
                // Hardik MailChimp trigger for subscription request
                await _mailChimpService.AddNewSubscriberAsync(ConfigurationManager.AppSettings["ListID"], member);
                //var q = await _mailChimpService.SendUpdateNotification(customerFormData.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }

        public async Task SendHomeImprovementMailToCustomer(IList<Contract> succededContracts)
        {
            string domain = ConfigurationManager.AppSettings["CustomerWalletClient"];
            var contract = succededContracts.First();
            string newemailid = _contractRepository.GetCustomer(contract.PrimaryCustomer.Id).Emails.FirstOrDefault(m => m.EmailType == EmailType.Main).EmailAddress;
            string services = string.Join(",", succededContracts.Select(i => (i.Equipment.NewEquipment.First()?.Description ?? 
                _contractRepository.GetEquipmentTypeInfo(i.Equipment.NewEquipment.First()?.Type)?.Description)?.ToLower()));
            string customerEmail = contract.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string hashLogin = SecurityUtils.Hash(customerEmail);
            string mbPhone = ConfigurationManager.AppSettings["CustomerWalletPhone"];
            string mbEmail = ConfigurationManager.AppSettings["CustomerWalletEmail"];

            var phoneIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-phone.png"));
            var phoneImage = GenerateIconImageCid(phoneIcon);
            var emailIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-email.png"));
            var emailImage = GenerateIconImageCid(emailIcon);

            var bottomStyle = "style ='font-size: 10px; !important'";
            var pStyle = "style='font-size: 18px; !important'";

            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.Hi} {contract.PrimaryCustomer.FirstName},</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.ThanksForYourInterestInHomeImprovementService} ({services}) {Resources.Resources.OnThe} {domain}.</p>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.WeAreNowLookingForheBest}</p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p>{Resources.Resources.InCaseOfQuestionsPleaseContact} <b>EcoHome Financial</b>  {Resources.Resources.Support.ToLower()}:</p>");
            body.AppendLine($"<p><img src='{phoneImage}'>{mbPhone}</p>");
            body.AppendLine($"<p><img src='{emailImage}'/> <a href='mailto:{mbEmail}'><span>{mbEmail}</span></a></li></p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p {bottomStyle}><b>This email was sent by EcoHome Financial</b> | 325 Milner Avenue, Suite 300 | Toronto, Ontario | M1B 5N1 Canada</p>");
            body.AppendLine($"<p {bottomStyle}><b>Contact us:</b> {mbPhone} | {mbEmail}</p>");
            body.AppendLine($"<p {bottomStyle}>We truly hope you found this message useful. However, if you'd rather not receive future e-mails of this sort from EcoHome Financial, please <b><a href='{domain}/unsubscribe/{hashLogin}'><span>click here to unsubscribe.</span></a></b>.</p>");
            body.AppendLine("</div>");

            var alternateView = GenerateAlternateView(body,  new List<LinkedResource>(){phoneIcon, emailIcon});

            var subject = $"{Resources.Resources.WeAreLookingForTheBestProfessionalForYourHomeImprovementProject}";
            var mail = GenerateMailMessage(customerEmail, subject, alternateView);
            //Hardik Update MailChimp Home Improvement type
            //MailChimpMember member = new MailChimpMember()
            //{
            //    Email = contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
            //    FirstName = contract.PrimaryCustomer.FirstName,
            //    LastName = contract.PrimaryCustomer.LastName,
            //    address = new MemberAddress()
            //    {
            //        Street = contract.PrimaryCustomer.Locations.FirstOrDefault().Street,
            //        Unit = contract.PrimaryCustomer.Locations.FirstOrDefault().Unit,
            //        City = contract.PrimaryCustomer.Locations.FirstOrDefault().City,
            //        State = contract.PrimaryCustomer.Locations.FirstOrDefault().State,
            //        PostalCode = contract.PrimaryCustomer.Locations.FirstOrDefault().PostalCode
            //    },
            //    CreditAmount = (decimal)contract.Details.CreditAmount,
            //    ApplicationStatus = contract.ContractState.ToString(),
            //    // TemporaryPassword = password,
            //    EquipmentInfoRequired = "Updated"
            //    //EquipmentInfoRequired = (contract.Equipment.NewEquipment.FirstOrDefault().Type == null) ? "Required" : "Not Required"
            //};
            try
            {
                // await _emailService.SendAsync(mail);
                // Hardik Mailchimp trigger to update Equipment type
                if (await _mailChimpService.isSubscriber(ConfigurationManager.AppSettings["ListID"], contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress))
                {
                    await _mandrillService.SendHomeImprovementTypeUpdatedConfirmation(contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                                                                                        contract.PrimaryCustomer.FirstName,
                                                                                        contract.PrimaryCustomer.LastName,
                                                                                        services);



                }
                var result = await _personalizedMessageService.SendMessage(contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell).PhoneNum, subject);

            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }
        public async Task SendDeclinedConfirmation(string emailid, string firstName, string lastName)
        {
            try
            {
                await _mandrillService.SendDeclineNotificationConfirmation(emailid, firstName, lastName);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task SendApprovedMailToCustomer(Contract customerFormData)
        {
            string customerEmail = customerFormData.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string domain = ConfigurationManager.AppSettings["CustomerWalletClient"];
            string hashLogin = SecurityUtils.Hash(customerEmail);
            string mbPhone = ConfigurationManager.AppSettings["CustomerWalletPhone"];
            string mbEmail = ConfigurationManager.AppSettings["CustomerWalletEmail"];

            var phoneIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-phone.png"));
            var phoneImage = GenerateIconImageCid(phoneIcon);
            var emailIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-email.png"));
            var emailImage = GenerateIconImageCid(emailIcon);

            var bottomStyle = "style='font-size: 10px; !important'";
            var pStyle = "style='font-size: 18px; !important'";

            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.Hi} {customerFormData.PrimaryCustomer.FirstName},</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.Congratulations}, {Resources.Resources.YouHaveBeen} <b>{Resources.Resources.PreApproved.ToLower()} {Resources.Resources.For} ${customerFormData.Details.CreditAmount.Value.ToString("N0", CultureInfo.InvariantCulture)}</b>.</p>");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.YouCanViewYourAccountOn} <b><a href='{domain}'><span>{domain}</span></a></b></p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p>{Resources.Resources.InCaseOfQuestionsPleaseContact} <b>EcoHome Financial</b>  {Resources.Resources.Support.ToLower()}:</p>");
            body.AppendLine($"<p><img src='{phoneImage}'>{mbPhone}</p>");
            body.AppendLine($"<p><img src='{emailImage}'/> <a href='mailto:{mbEmail}'><span>{mbEmail}</span></a></li></p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p {bottomStyle}><b>This email was sent by EcoHome Financial</b> | 325 Milner Avenue, Suite 300 | Toronto, Ontario | M1B 5N1 Canada</p>");
            body.AppendLine($"<p {bottomStyle}><b>Contact us:</b> {mbPhone} | {mbEmail}</p>");
            body.AppendLine($"<p {bottomStyle}>We truly hope you found this message useful. However, if you'd rather not receive future e-mails of this sort from EcoHome Financial, please <b><a href='{domain}/unsubscribe/{hashLogin}'><span>click here to unsubscribe.</span></a></b>.</p>");

            body.AppendLine("</div>");

            var alternateView = GenerateAlternateView(body, new List<LinkedResource>() { phoneIcon, emailIcon });

            var subject = $"{Resources.Resources.Congratulations}, {Resources.Resources.YouHaveBeen} {Resources.Resources.PreApproved.ToLower()} {Resources.Resources.For} ${customerFormData.Details.CreditAmount.Value.ToString("N0", CultureInfo.InvariantCulture)}";
            var mail = GenerateMailMessage(customerEmail, subject, alternateView);
            MailChimpMember member = new MailChimpMember()
            {
                Email = customerFormData.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                FirstName = customerFormData.PrimaryCustomer.FirstName,
                LastName = customerFormData.PrimaryCustomer.LastName,
                address = new MemberAddress()
                {
                    Street = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().Street,
                    Unit = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().Unit,
                    City = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().City,
                    State = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().State,
                    PostalCode = customerFormData.PrimaryCustomer.Locations.FirstOrDefault().PostalCode
                },
                CreditAmount = (decimal)customerFormData.Details.CreditAmount,
                ApplicationStatus = customerFormData.ContractState.ToString(),
                //TemporaryPassword = password,
               // EquipmentInfoRequired = "Required"
                //EquipmentInfoRequired = (customerFormData.Equipment.NewEquipment.FirstOrDefault().Type == null) ? "Required" : "Not Required"
            };

            try
            {
                //await _emailService.SendAsync(mail);
                //Hardik MailChimp Trigger to update CreditAmount
                await _mailChimpService.AddNewSubscriberAsync(ConfigurationManager.AppSettings["ListID"], member);
                var result = await _personalizedMessageService.SendMessage(customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell).PhoneNum, subject);

            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }

        public async Task SendCustomerDealerAcceptLead(Contract contract, DealerDTO dealer)
        {
            var location = dealer.Locations?.FirstOrDefault();
            var addres = location != null ? $"{location.Street}, {location.City}, {location.State}, {location.PostalCode}" : "";

            string customerEmail = contract.PrimaryCustomer.Emails?.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string domain = ConfigurationManager.AppSettings["CustomerWalletClient"];
            string hashLogin = SecurityUtils.Hash(customerEmail);
            string services = contract.Equipment.NewEquipment != null ? string.Join(",", contract.Equipment.NewEquipment.Select(i => (i.Description ?? _contractRepository.GetEquipmentTypeInfo(i?.Type)?.Description)?.ToLower())) : string.Empty;
            string mbPhone = ConfigurationManager.AppSettings["CustomerWalletPhone"];
            string mbEmail = ConfigurationManager.AppSettings["CustomerWalletEmail"];

            var phoneIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-phone.png"));
            var phoneImage = GenerateIconImageCid(phoneIcon);
            var emailIcon = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-email.png"));
            var emailImage = GenerateIconImageCid(emailIcon);

            var bottomStyle = "style='font-size: 10px; !important'";
            var pStyle = "style='font-size: 18px; !important'";

            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.Hi} {contract.PrimaryCustomer.FirstName},</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<h4>{Resources.Resources.WeFoundHomeProfessionalForYour} ({services}) - {contract.Dealer.DisplayName} {Resources.Resources.WillContactYouSoonText}.</h4>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p {pStyle}>{Resources.Resources.IfYouHaveQuestionsToHomeProfessionalPleaseContact} <b>{contract.Dealer.DisplayName}.</b></p>");
            if (!string.IsNullOrEmpty(addres))
            {
                body.AppendLine($"<p {pStyle}>{addres}</p>");
            }
            if (dealer.Phones?.FirstOrDefault()!=null)
            {
                body.AppendLine($"<p {pStyle}>{Resources.Resources.Phone}: {dealer.Phones.First().PhoneNum}</p>");
            }
            if (dealer.Emails?.FirstOrDefault() !=null)
            {
                body.AppendLine($"<p {pStyle}>{Resources.Resources.Mail}: {dealer.Emails.FirstOrDefault().EmailAddress}</p>");
            }
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p>{Resources.Resources.InCaseRemarksAboutSelectedHomeProfessionalsPleaseContact} <b>EcoHome Financial</b>  {Resources.Resources.Support.ToLower()}:</p>");
            body.AppendLine($"<p><img src='{phoneImage}'>{mbPhone}</p>");
            body.AppendLine($"<p><img src='{emailImage}'/> <a href='mailto:{mbEmail}'><span>{mbEmail}</span></a></li></p>");
            body.AppendLine("<br />");
            body.AppendLine("<br />");
            body.AppendLine($"<p {bottomStyle}><b>This email was sent by EcoHome Financial</b> | 325 Milner Avenue, Suite 300 | Toronto, Ontario | M1B 5N1 Canada</p>");
            body.AppendLine($"<p {bottomStyle}><b>Contact us:</b> {mbPhone} | {mbEmail}</p>");
            body.AppendLine($"<p {bottomStyle}>We truly hope you found this message useful. However, if you'd rather not receive future e-mails of this sort from EcoHome Financial, please <b><a href='{domain}/unsubscribe/{hashLogin}'><span>click here to unsubscribe.</span></a></b>.</p>");
            body.AppendLine("</div>");

            var alternateView = GenerateAlternateView(body, new List<LinkedResource>() { phoneIcon, emailIcon });

            var subject = $"{Resources.Resources.WeFoundHomeProfessionalForYourHomeImprovementProject}";
            var mail = GenerateMailMessage(customerEmail, subject, alternateView);
            MailChimpMember member = new MailChimpMember()
            {
                Email = contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                FirstName = contract.PrimaryCustomer.FirstName,
                LastName = contract.PrimaryCustomer.LastName,
                address = new MemberAddress()
                {
                    Street = contract.PrimaryCustomer.Locations.FirstOrDefault().Street,
                    Unit = contract.PrimaryCustomer.Locations.FirstOrDefault().Unit,
                    City = contract.PrimaryCustomer.Locations.FirstOrDefault().City,
                    State = contract.PrimaryCustomer.Locations.FirstOrDefault().State,
                    PostalCode = contract.PrimaryCustomer.Locations.FirstOrDefault().PostalCode
                },
                CreditAmount = (decimal)contract.Details.CreditAmount,
                ApplicationStatus = contract.ContractState.ToString(),
                DealerLeadAccepted = "Accepted",
                TemporaryPassword = "",
                //  EquipmentInfoRequired = "Required",
                OneTimeLink = domain + "/invite/" + hashLogin
                //EquipmentInfoRequired = (customerFormData.Equipment.NewEquipment.FirstOrDefault().Type == null) ? "Required" : "Not Required"

            };
            try
            {
                //Need to plug mailchimp 
                //await _emailService.SendAsync(mail);
               
                //await _mailChimpService.AddNewSubscriberAsync(ConfigurationManager.AppSettings["ListID"], member);
                var result = await _personalizedMessageService.SendMessage(contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell).PhoneNum, subject);
                if (await _mailChimpService.isSubscriber(ConfigurationManager.AppSettings["ListID"], contract.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress))
                {
                    
                    await _mandrillService.SendDealerLeadAccepted(contract, dealer, services);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }
        #endregion

        #region Public DealNet mails

        public async Task SendNotifyMailNoDealerAcceptLead(Contract contract)
        {
            string equipment = contract.Equipment.NewEquipment?.FirstOrDefault()?.Description.ToLower() ?? string.Empty;
            var location = contract.PrimaryCustomer.Locations?.FirstOrDefault(l=> l.AddressType == AddressType.InstallationAddress);
            string customerEmail = contract.PrimaryCustomer.Emails?.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string mailTo = ConfigurationManager.AppSettings["DealNetEmail"];
            var homePhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ?? string.Empty;
            var businessPhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ?? string.Empty;
            var mobilePhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ?? string.Empty;

            var body = new StringBuilder();
            body.AppendLine("<div>");
            body.AppendLine($"<u>{Resources.Resources.ThereAreNoDealersMatchingFollowingLead}.</u>");
            body.AppendLine($"<p>{Resources.Resources.TransactionId}: {contract.Details?.TransactionId ?? contract.Id.ToString()}</p>");
            body.AppendLine($"<p><b>{Resources.Resources.Client}: {contract.PrimaryCustomer.FirstName} {contract.PrimaryCustomer.LastName}</b></p>");
            body.AppendLine($"<p><b>{Resources.Resources.PreApproved}: ${contract.Details?.CreditAmount?.ToString("N0", CultureInfo.InvariantCulture)}</b></p>");
            body.AppendLine($"<p><b>{Resources.Resources.HomeImprovementType}: {equipment}</b></p>");
            if (!string.IsNullOrEmpty(contract.Details?.Notes))
            {
                body.AppendLine($"<p>{Resources.Resources.ClientsComment}: <i>{contract.Details.Notes}</i></p>");
            }
            body.AppendLine("<br />");
            body.AppendLine($"<p><b>{Resources.Resources.InstallationAddress}:</b></p>");
            body.AppendLine($"<p>{location?.Street ?? string.Empty}</p>");
            body.AppendLine($"<p>{location?.City ?? string.Empty}, {location?.State ?? string.Empty} {location?.PostalCode ?? string.Empty}</p>");
            body.AppendLine("<br />");
            body.AppendLine($"<p><b>{Resources.Resources.ContactInformation}:</b></p>");
            body.AppendLine("<ul>");
            if (!string.IsNullOrEmpty(homePhone))
            {
                body.AppendLine($"<li>{Resources.Resources.HomePhone}: {homePhone}</li>");
            }
            if (!string.IsNullOrEmpty(mobilePhone))
            {
                body.AppendLine($"<li>{Resources.Resources.MobilePhone}: {mobilePhone}</li>");
            }
            if (!string.IsNullOrEmpty(businessPhone))
            {
                body.AppendLine($"<li>{Resources.Resources.BusinessPhone}: {businessPhone}</li>");
            }
            if (!string.IsNullOrEmpty(customerEmail))
            {
                body.AppendLine($"<li>{Resources.Resources.EmailAddress}: {customerEmail}</li>");
            }
            if (contract.PrimaryCustomer.PreferredContactMethod.HasValue)
            {
                body.AppendLine($"<li>{Resources.Resources.PreferredContactMethod}: {contract.PrimaryCustomer.PreferredContactMethod.Value}</li>");
            }
            body.AppendLine("</ul>");
            body.AppendLine("</div>");

            var subject = string.Format(Resources.Resources.NoDealersMatchingCustomerLead, equipment, location?.PostalCode ?? string.Empty);
            try
            {
                await _emailService.SendAsync(new List<string> { mailTo }, string.Empty, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }

        public void SendNotifyMailNoDealerAcceptedLead12H(Contract contract)
        {
            string equipment = contract.Equipment.NewEquipment?.First().Description.ToLower() ?? string.Empty;
            var location = contract.PrimaryCustomer.Locations?.FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress);
            string customerEmail = contract.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            string mailTo = ConfigurationManager.AppSettings["DealNetEmail"];
            var homePhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ?? string.Empty;
            var businessPhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ?? string.Empty;
            var mobilePhone = contract?.PrimaryCustomer?.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ?? string.Empty;
            var expireperiod = int.Parse(ConfigurationManager.AppSettings["LeadExpiredMinutes"]) / 60;

            var body = new StringBuilder();
            body.AppendLine("<div>");
            body.AppendLine($"<u>{string.Format(Resources.Resources.FollowingLeadHasNotBeenAcceptedByAnyDealerFor12h, expireperiod)}.</u>");
            body.AppendLine($"<p>{Resources.Resources.TransactionId}: {contract.Details?.TransactionId ?? contract.Id.ToString()}</p>");
            body.AppendLine($"<p><b>{Resources.Resources.Client}: {contract.PrimaryCustomer.FirstName} {contract.PrimaryCustomer.LastName}</b></p>");
            body.AppendLine($"<p><b>{Resources.Resources.PreApproved}: ${contract.Details.CreditAmount.Value.ToString("N0", CultureInfo.InvariantCulture)}</b></p>");
            body.AppendLine($"<p><b>{Resources.Resources.HomeImprovementType}: {equipment}</b></p>");
            if (!string.IsNullOrEmpty(contract.Details?.Notes))
            {
                body.AppendLine($"<p>{Resources.Resources.ClientsComment}: <i>{contract.Details.Notes}</i></p>");
            }
            body.AppendLine("<br />");
            body.AppendLine($"<p><b>{Resources.Resources.InstallationAddress}:</b></p>");
            body.AppendLine($"<p>{location?.Street ?? string.Empty}</p>");
            body.AppendLine($"<p>{location?.City ?? string.Empty}, {location?.State ?? string.Empty} {location?.PostalCode ?? string.Empty}</p>");
            body.AppendLine("<br />");
            body.AppendLine($"<p><b>{Resources.Resources.ContactInformation}:</b></p>");
            body.AppendLine("<ul>");
            if (!string.IsNullOrEmpty(homePhone))
            {
                body.AppendLine($"<li>{Resources.Resources.HomePhone}: {homePhone}</li>");
            }
            if (!string.IsNullOrEmpty(mobilePhone))
            {
                body.AppendLine($"<li>{Resources.Resources.MobilePhone}: {mobilePhone}</li>");
            }
            if (!string.IsNullOrEmpty(businessPhone))
            {
                body.AppendLine($"<li>{Resources.Resources.BusinessPhone}: {businessPhone}</li>");
            }
            if (!string.IsNullOrEmpty(customerEmail))
            {
                body.AppendLine($"<li>{Resources.Resources.EmailAddress}: {customerEmail}</li>");
            }
            if (contract.PrimaryCustomer.PreferredContactMethod.HasValue)
            {
                body.AppendLine($"<li>{Resources.Resources.PreferredContactMethod}: {contract.PrimaryCustomer.PreferredContactMethod.Value}</li>");
            }
            body.AppendLine("</ul>");
            body.AppendLine("</div>");

            

            var subject = string.Format(Resources.Resources.CustomerLeadHasNotBeenAcceptedByAnyDealerFor, expireperiod, equipment, location?.PostalCode ?? string.Empty);
            try
            {
                _emailService.SendAsync(new List<string> { mailTo }, string.Empty, subject, body.ToString());
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Cannot send email", ex);
            }
        }

        #endregion

        #region Private
        private async Task SendNotification(string body, string subject, ContractDTO contract, string dealerEmail, List<Alert> alerts)
        {
            if (contract != null)
            {
                var recipients = GetContractRecipients(contract, dealerEmail);

                if (recipients.Any())
                {
                    try
                    {
                        foreach (var recipient in recipients)
                        {
                            await _emailService.SendAsync(new List<string>() { recipient }, string.Empty, subject, body);
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Can't send notification email";
                        _loggingService.LogError("Can't send notification email", ex);
                        alerts.Add(new Alert()
                        {
                            Header = errorMsg,
                            Message = ex.ToString(),
                            Type = AlertType.Error
                        });
                    }
                }
                else
                {
                    var errorMsg = $"Can't get recipients list for contract [{contract.Id}]";
                    _loggingService.LogError(errorMsg);
                    alerts.Add(new Alert()
                    {
                        Header = "Can't get recipients list",
                        Message = errorMsg,
                        Type = AlertType.Error
                    });
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Header = "Can't get contract",
                    Message = $"Can't get contract with id {contract.Id}",
                    Type = AlertType.Error
                });
                _loggingService.LogError($"Can't get contract with id {contract.Id}");
            }

            if (alerts.All(a => a.Type != AlertType.Error))
            {
                _loggingService.LogInfo($"Email notifications for contract [{contract.Id}] was sent");
            }
        }
        
        private AlternateView GenerateAlternateView(StringBuilder body, IList<LinkedResource> iconResources)
        {
            var alternateView = AlternateView.CreateAlternateViewFromString(body.ToString(), null, MediaTypeNames.Text.Html);
            iconResources.ForEach(icon => alternateView.LinkedResources.Add(icon));
            return alternateView;
        }

        private MailMessage GenerateMailMessage(string customerEmail, string subject, AlternateView alternateView )
        {
            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.AlternateViews.Add(alternateView);
            mail.From = new MailAddress(ConfigurationManager.AppSettings["EmailService.FromEmailAddress"]);
            mail.To.Add(customerEmail);
            mail.Subject = subject;
            return mail;
        }

        private IList<string> GetContractRecipients(ContractDTO contract, string dealerEmail)
        {
            var recipients = new List<string>();

            if (contract.PrimaryCustomer?.Emails?.Any() ?? false)
            {
                recipients.Add(contract.PrimaryCustomer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                    contract.PrimaryCustomer.Emails.First().EmailAddress);
            }

            if (contract?.SecondaryCustomers?.Any() ?? false)
            {
                contract.SecondaryCustomers.ForEach(c =>
                {
                    if (c.Emails?.Any() ?? false)
                    {
                        recipients.Add(c.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                            c.Emails.First().EmailAddress);
                    }
                });
            }

            //TODO: dealer and ODI/Ecohome team
            if (!string.IsNullOrEmpty(dealerEmail))
            {
                recipients.Add(dealerEmail);
            }

            return recipients;
        }

        private string GenerateIconImageCid(LinkedResource icon)
        {
            icon.ContentId = Guid.NewGuid().ToString();
            icon.ContentType.MediaType = "image/png";
            var emailImage = "cid:" + icon.ContentId;
            return emailImage;
        }
        #endregion
    }
}
