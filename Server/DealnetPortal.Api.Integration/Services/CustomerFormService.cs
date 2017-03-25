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
        private readonly IDealerRepository _dealerRepository;
        private readonly ILoggingService _loggingService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository, IUnitOfWork unitOfWork,
            IDealerRepository dealerRepository, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
            _dealerRepository = dealerRepository;
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
                    IsLanguageEnabled = linkSettings.EnabledLanguages.FirstOrDefault(l => l.Language?.Code == language) != null
                };
                if (langSettings.IsLanguageEnabled)
                {
                    langSettings.LanguageServices =
                        linkSettings.Services.Where(
                            s => s.LanguageId == linkSettings.EnabledLanguages.First(l => l.Language?.Code == language).LanguageId)
                            .Select(s => s.Service).ToList();
                }                
                langSettings.EnabledLanguages =
                        linkSettings.EnabledLanguages.Select(l => (LanguageCode)l.LanguageId).ToList();
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
            if (customerFormData == null)
            {
                throw new ArgumentNullException(nameof(customerFormData));
            }
            var alerts = new List<Alert>();

            var dealerId = _dealerRepository.GetUserIdByName(customerFormData.DealerName);
            if (!string.IsNullOrEmpty(dealerId))
            {
                var contract = _contractRepository.CreateContract(dealerId);
                if (contract != null)
                {
                    contract.IsCreatedByCustomer = true;
                    _unitOfWork.Save();

                    var customer = Mapper.Map<Customer>(customerFormData.PrimaryCustomer);
                    var contractData = new ContractData()
                    {
                        PrimaryCustomer = customer,
                        HomeOwners = new List<Customer> {customer},
                        DealerId = dealerId,
                        Id = contract.Id
                    };

                    contract = _contractRepository.UpdateContractData(contractData, dealerId);
                    var customerContractInfo = new CustomerContractInfo()
                    {

                    };
                    _customerFormRepository.AddCustomerContractData(customerContractInfo);
                    _unitOfWork.Save();
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Code = ErrorCodes.ContractCreateFailed,
                        Header = "Cannot create contract",
                        Message = "Cannot create contract from customer loan form"
                    });
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.CantGetUserFromDb,
                    Message = $"Cannot get dealer {customerFormData.DealerName} from database",
                    Header = "Cannot get dealer from database"
                });
            }
            return alerts;
        }        
    }
}
