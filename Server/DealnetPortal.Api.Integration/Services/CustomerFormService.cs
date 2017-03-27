using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
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

        public async Task<IList<Alert>> SubmitCustomerFormData(CustomerFormDTO customerFormData)
        {
            var alerts = new List<Alert>();
            if (customerFormData != null)
            {
                try
                {
                    var dealer = _aspireStorageService.GetDealerInfo(customerFormData.DealerName);
                    var dealerColor =
                        _settingsRepository.GetUserStringSettings(customerFormData.DealerName)
                            .FirstOrDefault(s => s.Item.Name == "@navbar-header");
                    var dealerLogo = _settingsRepository.GetUserBinarySetting(SettingType.LogoImage2X,
                        customerFormData.DealerName);
                    //Uncomment if fake dealer is required
                    //dealer = new DealerDTO()
                    //{
                    //    Emails = new List<EmailDTO>
                    //    {
                    //        new EmailDTO
                    //        {
                    //            EmailAddress = "john.doe@dataart.com",
                    //            EmailType = EmailType.Main
                    //        }
                    //    },
                    //    Locations = new List<LocationDTO>
                    //    {
                    //        new LocationDTO
                    //        {
                    //            AddressType = AddressType.MainAddress,
                    //            City = "Odesa",
                    //            State = "Odeska",
                    //            PostalCode = "65000",
                    //            Street = "Deribasivska 1"
                    //        }
                    //    },
                    //    Phones = new List<PhoneDTO>
                    //    {
                    //        new PhoneDTO
                    //        {
                    //            PhoneNum = "+38025252525",
                    //            PhoneType = PhoneType.Home
                    //        }
                    //    },
                    //    FirstName = "John Doe"
                    //};
                    //
                    try
                    {
                        await SendDealerSubmitNotification(dealer?.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress,
                                customerFormData, null); //TODO: Get pre-approved amount
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Can't send dealer notification email";
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Warning,
                            Message = errorMsg
                        });
                        _loggingService.LogError(errorMsg, ex);
                    }
                    //
                    try
                    {
                        await
                            SendCustomerSubmitNotification(customerFormData.PrimaryCustomer.Emails.FirstOrDefault(
                                    m => m.EmailType == EmailType.Main)?.EmailAddress, null, dealer, //TODO: Get pre-approved amount
                                    dealerColor?.StringValue, dealerLogo?.BinaryValue);
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Can't send customer notification email";
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Warning,
                            Message = errorMsg
                        });
                        _loggingService.LogError(errorMsg, ex);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = "Can't retrieve dealer info";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Warning,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg, ex);

                }
            }
            else
            {
                var errorMsg = "No customer form data";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.CustomerFormSubmitFailed,
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        private async Task SendDealerSubmitNotification(string dealerEmail, CustomerFormDTO customerFormData, double? preapprovedAmount)
        {
            var address = string.Empty;
            var addresItem = customerFormData.PrimaryCustomer.Locations.FirstOrDefault(ad => ad.AddressType == AddressType.MainAddress);

            if (addresItem != null)
            {
                address = $"{addresItem.Street}, {addresItem.City}, {addresItem.PostalCode}, {addresItem.State}";
            }
            var body = new StringBuilder();
            body.AppendLine($"<h3>{Resources.Resources.NewCustomerAppliedForFinancing}</h3>");
            body.AppendLine("<div>");
            body.AppendLine($"<p>{Resources.Resources.ContractId}: {Resources.Resources.IDNotYetGenerated}</p>");//todo:Check does it need?
            body.AppendLine($"<p><b>{Resources.Resources.Name}: {$"{customerFormData.PrimaryCustomer.FirstName} {customerFormData.PrimaryCustomer.LastName}"}</b></p>");
            body.AppendLine($"<p><b>{Resources.Resources.PreApproved}: Amount from Espire</b></p>");//todo: Need to get this amount from espire
            body.AppendLine($"<p><b>{Resources.Resources.SelectedTypeOfService}: {customerFormData.SelectedService ?? string.Empty}</b></p>");
            body.AppendLine($"<p>{Resources.Resources.Comment}: {customerFormData.CustomerComment}</p>");
            body.AppendLine($"<p>{Resources.Resources.InstallationAddress}: {address}</p>");
            body.AppendLine($"<p>{Resources.Resources.HomePhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.CellPhone}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.InstallationAddress}: {customerFormData.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business)?.PhoneNum ?? string.Empty}</p>");
            body.AppendLine($"<p>{Resources.Resources.Email}: {customerFormData.PrimaryCustomer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty}</p>");
            body.AppendLine("</div>");

            var message = new IdentityMessage()
            {
                Body = body.ToString(),
                Subject = Resources.Resources.NewCustomerAppliedForFinancing,
                Destination = dealerEmail ?? string.Empty
            };
            await _emailService.SendAsync(message);
        } 

        private async Task SendCustomerSubmitNotification(string customerEmail, double? preapprovedAmount, DealerDTO dealer, string dealerColor, byte[] dealerLogo)
        {
            var dealerName = $"{dealer.FirstName} {dealer.LastName}";
            var email = dealer.Emails.FirstOrDefault(m => m.EmailType == EmailType.Main)?.EmailAddress ?? string.Empty;
            var location = dealer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress);
            var phone = dealer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home)?.PhoneNum;
            var html = File.ReadAllText(HostingEnvironment.MapPath(@"~\Content\emails\customer-notification-email.html"));
            var body = new StringBuilder(html, html.Length * 2);
            body.Replace("{headerColor}", dealerColor ?? "#000000");
            body.Replace("{thankYouForApplying}", Resources.Resources.ThankYouForApplyingForFinancing);
            body.Replace("{youHaveBeenPreapprovedFor}", preapprovedAmount != null ? Resources.Resources.YouHaveBeenPreapprovedFor.Replace("{0}", preapprovedAmount.ToString()) : string.Empty);
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

            LinkedResource inlineLogo = null; 
            var inlineSuccess = new LinkedResource(HostingEnvironment.MapPath(@"~\Content\emails\images\icon-success.png"));
            inlineSuccess.ContentId = Guid.NewGuid().ToString();
            inlineSuccess.ContentType.MediaType = "image/png";
            body.Replace("{successIcon}", "cid:" + inlineSuccess.ContentId);
            if (dealerLogo != null)
            {
                inlineLogo = new LinkedResource(new MemoryStream(dealerLogo));
                inlineLogo.ContentId = Guid.NewGuid().ToString();
                inlineLogo.ContentType.MediaType = "image/png";
                body.Replace("{dealerLogo}", "cid:" + inlineLogo.ContentId);
            }
            var alternateView = AlternateView.CreateAlternateViewFromString(body.ToString(), null,
                    MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(inlineSuccess);
            if (inlineLogo != null)
            {
                alternateView.LinkedResources.Add(inlineLogo);
            }

            var mail = new MailMessage();
            mail.IsBodyHtml = true;
            mail.AlternateViews.Add(alternateView);
            mail.From = new MailAddress(email);
            mail.To.Add(customerEmail);
            //mail.Subject = "yourSubject"; //TODO: Clarify subject
            await _emailService.SendAsync(mail);
        }
    }
}
