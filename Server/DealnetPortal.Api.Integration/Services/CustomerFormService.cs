using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository, IUnitOfWork unitOfWork,
            ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
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
    }
}
