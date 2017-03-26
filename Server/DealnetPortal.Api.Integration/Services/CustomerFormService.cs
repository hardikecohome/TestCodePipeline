using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;

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

            throw new NotImplementedException();
            var alerts = new List<Alert>();

            if (customerFormData != null)
            {
                
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
