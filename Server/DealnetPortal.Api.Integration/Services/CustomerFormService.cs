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
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Logging;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Api.Integration.Services
{
    public class CustomerFormService : ICustomerFormService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ICustomerFormRepository _customerFormRepository;
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly IMailService _mailService;
        private readonly ISettingsRepository _settingsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDealerRepository _dealerRepository;
        private readonly IContractService _contractService;
        private readonly ILoggingService _loggingService;

        public CustomerFormService(IContractRepository contractRepository, ICustomerFormRepository customerFormRepository,
            IDealerRepository dealerRepository, ISettingsRepository settingsRepository, IUnitOfWork unitOfWork, IContractService contractService,
            ILoggingService loggingService, IMailService mailService, IAspireStorageReader aspireStorageReader)
        {
            _contractRepository = contractRepository;
            _customerFormRepository = customerFormRepository;
            _dealerRepository = dealerRepository;
            _aspireStorageReader = aspireStorageReader;
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

        public CustomerLinkLanguageOptionsDTO GetCustomerLinkLanguageOptions(string hashDealerName, string language)
        {
            var linkSettings = _customerFormRepository.GetCustomerLinkSettingsByHashDealerName(hashDealerName);
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
                langSettings.DealerName = _dealerRepository.GetDealerNameByCustomerLinkId(linkSettings.Id);
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
                    updatedLink.HashLink = customerLinkSettings.HashLink;
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

        public async Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerFormData(CustomerFormDTO customerFormData)
        {
            if (customerFormData == null)
            {
                throw new ArgumentNullException(nameof(customerFormData));
            }

            var contractCreationRes = await CreateContractByCustomerFormData(customerFormData).ConfigureAwait(false);
            if (contractCreationRes?.Item1 != null &&
                (contractCreationRes.Item2?.All(a => a.Type != AlertType.Error) ?? true))
            {
                bool isCustomerCreator = false;
                var dealerId = customerFormData.DealerId ??
                               _dealerRepository.GetUserIdByName(customerFormData.DealerName);
                //for creation contract from 
                if (dealerId != null && _dealerRepository.GetUserRoles(dealerId).Contains(UserRole.CustomerCreator.ToString()))
                {
                    isCustomerCreator = true;
                }
                // will not wait end of this operation
                if (!isCustomerCreator)
                {
                    var noWarning = SendCustomerContractCreationNotifications(customerFormData,
                        contractCreationRes.Item1);
                }
            }

            return new Tuple<CustomerContractInfoDTO, IList<Alert>>(contractCreationRes?.Item1, contractCreationRes?.Item2 ?? new List<Alert>());
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
                        AccountId = contract.PrimaryCustomer?.AccountId,
                        DealerName = contract.LastUpdateOperator,
                        TransactionId = contract.Details?.TransactionId,
                        ContractState = contract.ContractState,
                        Status = contract.Details?.Status,
                        CreditAmount = contract.Details?.CreditAmount ?? 0,
                        ScorecardPoints = contract.Details?.ScorecardPoints ?? 0,
                        CreationTime = contract.CreationTime,
                        LastUpdateTime = contract.LastUpdateTime,
                        EquipmentTypes = contract.Equipment?.NewEquipment?.Select(e => e.Type).ToList()
                    };

                    try
                    {
                        //get dealer info
                        var dealer = Mapper.Map<DealerDTO>(_aspireStorageReader.GetDealerInfo(dealerName));
                        if (dealer != null)
                        {
                            contractInfo.DealerName = dealer.FirstName;
                            var dealerAddress = dealer.Locations?.FirstOrDefault();
                            if (dealerAddress != null)
                            {
                                contractInfo.DealerAdress = dealerAddress;
                            }
                            if (dealer.Phones?.Any() ?? false)
                            {
                                contractInfo.DealerPhone = dealer.Phones.First().PhoneNum;
                            }
                            if (dealer.Emails?.Any() ?? false)
                            {
                                contractInfo.DealerEmail = dealer.Emails.First().EmailAddress;
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
                Contract contract = null;
                //update or create a brand new contract
                if (customerFormData.PrecreatedContractId.HasValue)
                {
                    contract = _contractRepository.GetContract(customerFormData.PrecreatedContractId.Value, dealerId);
                    if (contract != null && contract.PrimaryCustomer.AccountId == customerFormData.PrimaryCustomer.AccountId)
                    {
                        _loggingService.LogInfo($"Selected contract [{contract.Id}] for update by customer loan form request for {customerFormData.DealerName} dealer");
                    }
                    else
                    {
                        contract = null;
                    }
                }
                if (contract == null)
                {
                    contract = _contractRepository.CreateContract(dealerId);
                    _unitOfWork.Save();
                    _loggingService.LogInfo($"Created new contract [{contract.Id}] by customer loan form request for {customerFormData.DealerName} dealer");
                }

                if (contract != null)
                {                                        
                    //var customer = Mapper.Map<Customer>(customerFormData.PrimaryCustomer);
                    var contractData = new ContractDataDTO()
                    {
                        PrimaryCustomer = customerFormData.PrimaryCustomer,
                        //HomeOwners = new List<Customer> { customer },
                        DealerId = dealerId,
                        Id = contract.Id
                    };
                    if (!string.IsNullOrEmpty(customerFormData.SelectedServiceType))
                    {
                        contractData.Equipment = new EquipmentInfoDTO()
                        {
                            NewEquipment =
                                new List<NewEquipmentDTO> {new NewEquipmentDTO() {Type = customerFormData.SelectedServiceType}}
                        };
                    }

                    //add coment from customer
                    if (!string.IsNullOrEmpty(customerFormData.CustomerComment))
                    {                        
                        try
                        {
                            if (!string.IsNullOrEmpty(customerFormData.SelectedService))
                            {
                                //if customerFormData.SelectedService - comes from customer form 
                                _customerFormRepository.AddCustomerContractData(contract.Id,
                                    $"{Resources.Resources.CommentFromCustomer}:",
                                    customerFormData.SelectedService, customerFormData.CustomerComment, dealerId);
                            }
                            else 
                            if (!string.IsNullOrEmpty(customerFormData.SelectedServiceType))
                            {
                                contractData = new ContractDataDTO()
                                {
                                    Id = contract.Id,
                                    Details = new ContractDetailsDTO()
                                    {
                                        Notes = customerFormData.CustomerComment
                                    }
                                };
                            }
                            _unitOfWork.Save();
                            _loggingService.LogInfo($"Customer's info is added to [{contract.Id}]");
                        }
                        catch (Exception ex)
                        {
                            var errorMsg =
                                $"Cannot update contract {contract.Id} from customer loan form with customer form data";
                            alerts.Add(new Alert()
                            {
                                Type = AlertType.Warning, //?
                                Code = ErrorCodes.ContractCreateFailed,
                                Header = "Cannot update contract",
                                Message = errorMsg
                            });
                            _loggingService.LogWarning(errorMsg);
                        }                        
                    }                               

                    //Do credit check only for new contract (not updated from CW)
                    if (contract.ContractState < ContractState.CreditContirmed)
                    {
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
                        }).ConfigureAwait(false);
                        if (creditCheckRes?.Item2?.Any() ?? false)
                        {
                            alerts.AddRange(creditCheckRes.Item2);
                        }
                    }

                    // mark as created by customer
                    contract.IsCreatedByCustomer = true;
                    contract.IsNewlyCreated = true;

                    _unitOfWork.Save();
                    submitResult = GetCustomerContractInfo(contract.Id, customerFormData.DealerName);
                    if (_contractRepository.IsContractUnassignable(contract.Id))
                    {
                        await _mailService.SendNotifyMailNoDealerAcceptLead(contract);
                    }
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

        private async Task SendCustomerContractCreationNotifications(CustomerFormDTO customerFormData, CustomerContractInfoDTO contractData)
        {
            var dealerColor = _settingsRepository.GetUserStringSettings(customerFormData.DealerName)
                                    .FirstOrDefault(s => s.Item.Name == "@navbar-header");
            var dealerLogo = _settingsRepository.GetUserBinarySetting(SettingType.LogoImage2X,
                customerFormData.DealerName);

            try
            {
                await
                    _mailService.SendDealerLoanFormContractCreationNotification(customerFormData, contractData); 
            }
            catch (Exception ex)
            {
                var errorMsg = "Can't send dealer notification email";
                _loggingService.LogError(errorMsg, ex);
            }
            
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
                                m => m.EmailType == EmailType.Main)?.EmailAddress, contractData, dealerColor?.StringValue, dealerLogo?.BinaryValue);
                }
                catch (Exception ex)
                {
                    var errorMsg = "Can't send customer notification email";
                    _loggingService.LogError(errorMsg, ex);
                }
            }
        }
    }
}
