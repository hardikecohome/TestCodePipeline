using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public class CustomerFormService : ICustomerFormService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICustomerFormRepository _customerFormRepository;
        private readonly IAspireStorageService _aspireStorageService;
        private readonly IEmailService _emailService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly IIdentityMessageService _emailService;
        private readonly IAspireStorageService _aspireStorageService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository, IUnitOfWork unitOfWork,
            ILoggingService loggingService, IAspireStorageService aspireStorageService, ISettingsRepository settingsRepository, IEmailService emailService)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
            _aspireStorageService = aspireStorageService;
            _settingsRepository = settingsRepository;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _emailService = emailService;
            _aspireStorageService = aspireStorageService;
        }

        public CustomerLinkDTO GetCustomerLinkSettings(string dealerId)
        {
            var linkSettings = _customerFormRepository.GetCustomerLinkSettings(dealerId);
            if (linkSettings != null)
            {
                return Mapper.Map<CustomerLinkDTO>(linkSettings);
            }
            return null;
        }

        public CustomerLinkDTO GetCustomerLinkSettingsByDealerName(string dealerName)
        {
            var linkSettings = _customerFormRepository.GetCustomerLinkSettingsByDealerName(dealerName);
            if (linkSettings != null)
            {
                return Mapper.Map<CustomerLinkDTO>(linkSettings);
            }
            return null;
        }

        public CustomerLinkLanguageOptionsDTO GetCustomerLinkLanguageOptions(string dealerName, string language)
        {
            var linkSettings = _customerFormRepository.GetCustomerLinkSettingsByDealerName(dealerName);
            if (linkSettings != null)
            {
                var langSettings = new CustomerLinkLanguageOptionsDTO
                {
                    IsLanguageEnabled = linkSettings.EnabledLanguages.FirstOrDefault(l => l.Code == language) != null
                };
                if (langSettings.IsLanguageEnabled)
                {
                    langSettings.LanguageServices =
                        linkSettings.Services.Where(
                            s => s.LanguageId == linkSettings.EnabledLanguages.First(l => l.Code == language).Id)
                            .Select(s => s.Service).ToList();
                }                
                langSettings.EnabledLanguages =
                        linkSettings.EnabledLanguages.Select(l => (LanguageCode)l.Id).ToList();
                return langSettings;
            }
            return null;
        }

        public IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId)
        {
            var alerts = new List<Alert>();
            try
            {
                var linkSettings = Mapper.Map<CustomerLink>(customerLinkSettings);
                CustomerLink updatedLink = null;
                if (linkSettings.EnabledLanguages != null)
                {
                    updatedLink = _customerFormRepository.UpdateCustomerLinkLanguages(linkSettings.EnabledLanguages, dealerId);
                }
                if (linkSettings.Services != null)
                {
                    updatedLink = _customerFormRepository.UpdateCustomerLinkServices(linkSettings.Services, dealerId) ?? updatedLink;
                }
                if (updatedLink != null)
                {
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to update a customer link settings for [{dealerId}] dealer", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.FailedToUpdateSettings,
                    Header = "Failed to update a customer link settings",
                    Message = "Failed to update a customer link settings"
                });
            }
            return alerts;
        }

        public IList<Alert> SubmitCustomerFormData(CustomerFormDTO customerFormData)
        {
            var alerts = new List<Alert>();
            if (customerFormData != null)
            {
                var address = string.Empty;
                var addresItem =customerFormData.PrimaryCustomer.Locations.FirstOrDefault(ad => ad.AddressType == AddressType.MainAddress);

                if (addresItem != null)
                {
                    address = string.Format("{0}, {1}, {2}, {3}", addresItem.Street, addresItem.City, addresItem.PostalCode, addresItem.State);
                }
                var body = new StringBuilder();
                body.AppendLine($"<h3>{Resources.Resources.NewCustomerAppliedForFinancing}</h3>");
                body.AppendLine("<div>");
                body.AppendLine($"<p>{Resources.Resources.ContractId}: {Resources.Resources.IDNotYetGenerated}</p>");//todo:Check does it need?
                body.AppendLine($"<p><b>{Resources.Resources.Name}: {string.Format("{0} {1}", customerFormData.PrimaryCustomer.FirstName, customerFormData.PrimaryCustomer.LastName)}</b></p>");
                body.AppendLine($"<p><b>{Resources.Resources.PreApproved}: Amount from Espire</b></p>");//todo: Need to get this amount from espire
                body.AppendLine($"<p><b>{Resources.Resources.SelectedTypeOfService}: {customerFormData.SelectedService ?? string.Empty}</b></p>");
                body.AppendLine($"<p>{Resources.Resources.Comment}: {customerFormData.CustomerComment}</p>");
                body.AppendLine($"<p>{Resources.Resources.InstallationAddress}: {address}</p>");
                body.AppendLine($"<p>{Resources.Resources.HomePhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ?? string.Empty}</p>");
                body.AppendLine($"<p>{Resources.Resources.CellPhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ?? string.Empty}</p>");
                body.AppendLine($"<p>{Resources.Resources.InstallationAddress}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ?? string.Empty}</p>");
                body.AppendLine($"<p>{Resources.Resources.Email}: {customerFormData.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty}</p>");
                body.AppendLine("</div>");
                var dealer = _aspireStorageService.GetDealerInfo(customerFormData.DealerName);
                var message = new IdentityMessage()
                {
                    Body = body.ToString(),
                    Subject = Resources.Resources.NewCustomerAppliedForFinancing,
                    Destination = dealer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty
                };
                _emailService.SendAsync(message);
            }
            else
            {
                var errorMsg = "Cannot find a contract";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractCreateFailed,
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        private async Task SendSubmitNotification(string customerEmail, double? preapprovedAmount, DealerDTO dealer, string dealerColor, byte[] dealerLogo)
        {
            var dealerName = $"{dealer.FirstName} {dealer.LastName}";
            var email = dealer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            var location = dealer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress);
            var phone = dealer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum;
            var html = File.ReadAllText(HostingEnvironment.MapPath(@"Content\Emails\customer-notification-email.html"));
            var body = new StringBuilder(html, html.Length * 2);
            body.Replace("{headerColor}", dealerColor ?? "#000000");
            body.Replace("{thankYouForApplying}", Resources.Resources.ThankYouForApplyingForFinancing);
            if (preapprovedAmount != null)
            {
                body.Replace("{youHaveBeenPreapprovedFor}", Resources.Resources.YouHaveBeenPreapprovedFor.Replace("{0}", preapprovedAmount.ToString()));
            }
            body.Replace("{yourApplicationWasSubmitted}", Resources.Resources.YourFinancingApplicationWasSubmitted);
            body.Replace("{willContactYouSoon}", Resources.Resources.WillContactYouSoon.Replace("{0}", dealerName));
            body.Replace("{ifYouHavePleaseContact}", Resources.Resources.IfYouHaveQuestionsPleaseContact);
            body.Replace("{dealerName}", dealerName);
            body.Replace("{dealerAddress}", $"{location?.Street} {location?.City}, {location?.State} {location?.PostalCode}");
            body.Replace("{phone}", Resources.Resources.Phone);
            body.Replace("{dealerPhone}", phone);
            body.Replace("{fax}", Resources.Resources.Fax);
            body.Replace("{dealerFax}", ""); //TODO: Get fax number
            body.Replace("{mail}", Resources.Resources.Email);
            body.Replace("{dealerMail}", email);

            AlternateView alternateView = null;
            if (dealerLogo != null)
            {
                var inline = new LinkedResource(new MemoryStream(dealerLogo));
                inline.ContentId = Guid.NewGuid().ToString();
                inline.ContentType.MediaType = "image/png";
                body.Replace("{dealerLogo}", "cid:" + inline.ContentId);
                alternateView = AlternateView.CreateAlternateViewFromString(body.ToString(), null,
                    MediaTypeNames.Text.Html);
                alternateView.LinkedResources.Add(inline);
            }

            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            if (alternateView != null)
            {
                mail.AlternateViews.Add(alternateView);
            }
            mail.From = new MailAddress(email);
            mail.To.Add(customerEmail);
            //mail.Subject = "yourSubject";
            await _emailService.SendAsync(mail);
        }
    }
}
