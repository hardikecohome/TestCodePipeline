﻿using System;
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

namespace DealnetPortal.Api.Integration.Services
{
    public class MailService : IMailService
    {
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;

        public MailService(IEmailService emailService, ILoggingService loggingService)
        {
            _emailService = emailService;
            _loggingService = loggingService;
        }

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

            try
            {
                await _emailService.SendAsync(new List<string> { contractData.DealerEmail ?? string.Empty }, string.Empty, Resources.Resources.NewCustomerAppliedForFinancing, body.ToString());
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
    }
}
