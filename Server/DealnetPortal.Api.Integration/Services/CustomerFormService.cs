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
        private readonly IMailService _mailService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDealerRepository _dealerRepository;
        private readonly IContractService _contractService;
        private readonly ILoggingService _loggingService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository,
            IDealerRepository dealerRepository, ISettingsRepository settingsRepository, IUnitOfWork unitOfWork, IContractService contractService,
            ILoggingService loggingService, IMailService mailService, IAspireStorageService aspireStorageService)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
            _dealerRepository = dealerRepository;
            _aspireStorageService = aspireStorageService;
            _settingsRepository = settingsRepository;
            _mailService = mailService;
            _unitOfWork = unitOfWork;
            _contractService = contractService;
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

        public async Task<Tuple<int?, IList<Alert>>> SubmitCustomerFormData(CustomerFormDTO customerFormData)
        {
            if (customerFormData == null)
            {
                throw new ArgumentNullException(nameof(customerFormData));
            }

            var contractCreationRes = await CreateContractByCustomerFormData(customerFormData);
            if (contractCreationRes?.Item1 != null &&
                (contractCreationRes.Item2?.All(a => a.Type != AlertType.Error) ?? true))
            {
                // will not wait end of this operation
                var noWarning = SendCustomerContractCreationNotifications(customerFormData, contractCreationRes.Item1?.CreditAmount ?? 0);
            }

            return new Tuple<int?, IList<Alert>>(contractCreationRes?.Item1?.ContractId, contractCreationRes?.Item2 ?? new List<Alert>());
        }

        public CustomerContractInfoDTO GetCustomerContractInfo(int contractId, string dealerName)
        {
            CustomerContractInfoDTO contractInfo = null;
            var dealerId = _dealerRepository.GetUserIdByName(dealerName);
            if (!string.IsNullOrEmpty(dealerId))
            {
                var contract = _contractRepository.GetContract(contractId, dealerId);
                if (contract != null)
                {
                    contractInfo = new CustomerContractInfoDTO()
                    {
                        ContractId = contractId,
                        TransactionId = contract.Details?.TransactionId,
                        ContractState = contract.ContractState,
                        Status = contract.Details?.Status,
                        CreditAmount = contract.Details?.CreditAmount ?? 0,
                        ScorecardPoints = contract.Details?.ScorecardPoints ?? 0,
                        CreationTime = contract.CreationTime,
                        LastUpdateTime = contract.LastUpdateTime
                    };

                    try
                    {
                        //get dealer info
                        var dealer = _aspireStorageService.GetDealerInfo(dealerName);
                        if (dealer != null)
                        {
                            contractInfo.DealerName = dealer.FirstName;
                            var dealerAddress = dealer.Locations?.FirstOrDefault();
                            if (dealerAddress != null)
                            {
                                contractInfo.DealerAdress = dealerAddress;
                                //                                        $"{dealerAddress.Street}, {dealerAddress.City}, {dealerAddress.State}, {dealerAddress.PostalCode}";
                            }
                            if (dealer.Phones?.Any() ?? false)
                            {
                                contractInfo.DealerPhone = dealer.Phones.First().PhoneNum;
                            }
                            if (dealer.Emails?.Any() ?? false)
                            {
                                contractInfo.DealerPhone = dealer.Emails.First().EmailAddress;
                            }
                            if (dealer.Emails?.Any() ?? false)
                            {
                                contractInfo.DealerPhone = dealer.Emails.First().EmailAddress;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = "Can't retrieve dealer info";
                        _loggingService.LogError(errorMsg, ex);
                    }

                }
            }
            return contractInfo;
        }

        private async Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> CreateContractByCustomerFormData(
            CustomerFormDTO customerFormData)
        {
            var alerts = new List<Alert>();
            CustomerContractInfoDTO submitResult = null;

            var dealerId = _dealerRepository.GetUserIdByName(customerFormData.DealerName);
            if (!string.IsNullOrEmpty(dealerId))
            {
                var contract = _contractRepository.CreateContract(dealerId);
                if (contract != null)
                {
                    contract.IsCreatedByCustomer = true;
                    _unitOfWork.Save();
                    _loggingService.LogInfo($"Created new contract [{contract.Id}] by customer loan form request for {customerFormData.DealerName} dealer");

                    var customer = Mapper.Map<Customer>(customerFormData.PrimaryCustomer);
                    var contractData = new ContractData()
                    {
                        PrimaryCustomer = customer,
                        HomeOwners = new List<Customer> { customer },
                        DealerId = dealerId,
                        Id = contract.Id
                    };

                    _contractRepository.UpdateContractData(contractData, dealerId);
                    _unitOfWork.Save();

                    var dealerSettings = _customerFormRepository.GetCustomerLinkSettings(dealerId);
                    DealerService service = null;
                    if (dealerSettings != null)
                    {
                        service = dealerSettings.Services.FirstOrDefault(s => s.Service == customerFormData.SelectedService);
                    }

                    var customerContractInfo = new CustomerContractInfo()
                    {
                        CustomerComment = customerFormData.CustomerComment,
                        SelectedServiceId = service?.Id
                    };
                    _customerFormRepository.AddCustomerContractData(contract.Id, customerContractInfo);
                    _unitOfWork.Save();

                    _loggingService.LogInfo($"Customer's info is added to [{contract.Id}]");

                    //Start credit check for this contract
                    var creditCheckRes = await Task.Run(() =>
                    {
                        var creditCheckAlerts = new List<Alert>();
                        var initAlerts = _contractService.InitiateCreditCheck(contract.Id, dealerId);
                        if (initAlerts?.Any() ?? false)
                        {
                            creditCheckAlerts.AddRange(initAlerts);
                        }
                        var checkResult = _contractService.GetCreditCheckResult(contract.Id, dealerId);
                        if (checkResult != null)
                        {
                            creditCheckAlerts.AddRange(checkResult.Item2);
                            return new Tuple<CreditCheckDTO, IList<Alert>>(checkResult.Item1, creditCheckAlerts);
                        }
                        return new Tuple<CreditCheckDTO, IList<Alert>>(null, creditCheckAlerts);
                    });

                    if (creditCheckRes?.Item2?.Any() ?? false)
                    {
                        alerts.AddRange(creditCheckRes.Item2);
                    }
                    if (creditCheckRes?.Item1 != null)
                    {
                        submitResult = new CustomerContractInfoDTO()
                        {
                            ContractId = contract.Id,
                            TransactionId = contract.Details?.TransactionId,
                            CreditAmount = creditCheckRes.Item1.CreditAmount,
                            ScorecardPoints = creditCheckRes.Item1.ScorecardPoints
                        };
                    }
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
                var errorMsg = $"Cannot get dealer {customerFormData.DealerName} from database";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.CantGetUserFromDb,
                    Message = errorMsg,
                    Header = "Cannot get dealer from database"
                });
                _loggingService.LogError(errorMsg);
            }
            if (alerts.Any(a => a.Type == AlertType.Error))
            {
                _loggingService.LogError("Cannot create contract by customer loan form request");
            }
            return new Tuple<CustomerContractInfoDTO, IList<Alert>>(submitResult, alerts);
        }

        private async Task SendCustomerContractCreationNotifications(CustomerFormDTO customerFormData, decimal creditCheckAmount)
        {
            //get dealer info
            DealerDTO dealer = null;
            try
            {
                dealer = _aspireStorageService.GetDealerInfo(customerFormData.DealerName);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Can't get information about {customerFormData.DealerName} dealer";
                //alerts.Add(new Alert()
                //{
                //    Type = AlertType.Warning,
                //    Message = errorMsg
                //});
                _loggingService.LogError(errorMsg, ex);
            }

            var dealerColor = _settingsRepository.GetUserStringSettings(customerFormData.DealerName)
                                    .FirstOrDefault(s => s.Item.Name == "@navbar-header");
            var dealerLogo = _settingsRepository.GetUserBinarySetting(SettingType.LogoImage2X,
                customerFormData.DealerName);

            try
            {
                await
                    _mailService.SendDealerLoanFormContractCreationNotification(
                        dealer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                        dealer?.Emails?.FirstOrDefault()?.EmailAddress,
                        customerFormData, (double)creditCheckAmount); //TODO: Get pre-approved amount
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't send dealer notification email";
                //alerts.Add(new Alert()
                //{
                //    Type = AlertType.Warning,
                //    Message = errorMsg
                //});
                _loggingService.LogError(errorMsg, ex);
            }
            //
            bool customerEmailNotification;
            bool.TryParse(ConfigurationManager.AppSettings["CustomerEmailNotificationEnabled"],
                out customerEmailNotification);
            if (customerEmailNotification)
            {
                try
                {
                    await
                        _mailService.SendCustomerLoanFormContractCreationNotification(
                            customerFormData.PrimaryCustomer.Emails.FirstOrDefault(
                                m => m.EmailType == EmailType.Main)?.EmailAddress, null, dealer,
                            //TODO: Get pre-approved amount
                            dealerColor?.StringValue, dealerLogo?.BinaryValue);
                }
                catch (Exception ex)
                {
                    var errorMsg = "Can't send customer notification email";
                    //alerts.Add(new Alert()
                    //{
                    //    Type = AlertType.Warning,
                    //    Message = errorMsg
                    //});
                    _loggingService.LogError(errorMsg, ex);
                }
            }
        }
    }
}
