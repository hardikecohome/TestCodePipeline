using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Domain.Repositories;
using Unity.Interception.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class MortgageBrokerService : IMortgageBrokerService
    {

        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAspireService _aspireService;
        private readonly ICustomerWalletService _customerWalletService;
        private readonly IMailService _mailService;
        private readonly ICreditCheckService _creditCheckService;
        private readonly IAppConfiguration _configuration;

        public MortgageBrokerService(IContractRepository contractRepository, ILoggingService loggingService, IUnitOfWork unitOfWork, IAspireService aspireService, ICustomerWalletService customerWalletService, IMailService mailService, IAppConfiguration configuration, ICreditCheckService creditCheckService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _aspireService = aspireService;
            _customerWalletService = customerWalletService;
            _mailService = mailService;
            _configuration = configuration;
            _creditCheckService = creditCheckService;
        }

        public IList<ContractDTO> GetCreatedContracts(string userId)
        {
            var contracts = _contractRepository.GetContractsCreatedByUser(userId);
            var contractDTOs = Mapper.Map<IList<ContractDTO>>(contracts, ctx =>
            {
                ctx.Items.Add(MappingKeys.EquipmentTypes, _contractRepository.GetEquipmentTypes());
            });
            return contractDTOs;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer)
        {
            try
            {
                var email = newCustomer.PrimaryCustomer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                       newCustomer.PrimaryCustomer.Emails.FirstOrDefault()?.EmailAddress;
                var checkCustomerAlerts = await _customerWalletService.CheckCustomerExisting(email);
                if (checkCustomerAlerts.Any())
                {
                    return new Tuple<ContractDTO, IList<Alert>>(null, checkCustomerAlerts);
                }

                var contractsResultList = new List<Tuple<Contract, bool>>();

                if (newCustomer.SelectedEquipmentTypes != null && newCustomer.SelectedEquipmentTypes.Any())
                {
                    foreach (var improvmentType in newCustomer.SelectedEquipmentTypes)
                    {
                        var result = await InitializeCreating(contractOwnerId, newCustomer, improvmentType);

                        contractsResultList.Add(result);
                    }
                }
                else
                {
                    contractsResultList.Add(await InitializeCreating(contractOwnerId, newCustomer));
                }

                if (contractsResultList.All(x => x.Item2 == false))
                {
                    var alerts = new List<Alert>();
                    _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}]");

                    var errorMsg = "Cannot create contract";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractCreateFailed,
                        Message = errorMsg
                    });
                    return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
                }

                //TODO: maybe it's better to try few times initiate contract check for unseccessful contracts
                var aspireFailedResults = new List<Tuple<int, bool>>();
                var creditCheckAlerts = new List<Alert>();

                foreach (var contractResult in contractsResultList.Where(x => x.Item1 != null).ToList())
                {
                    try
                    {
                        //try to submit deal in Aspire
                        //send only if HIT or Preffered start date are known
                        if (contractResult.Item1.Equipment?.PreferredStartDate != null || contractResult.Item1.Equipment?.NewEquipment?.Any() == true)
                        {
                            await _aspireService.SendDealUDFs(contractResult.Item1.Id, contractOwnerId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Cannot submit deal {contractResult.Item1.Id} in Aspire", ex);
                    }                    

                    var checkResult = _creditCheckService.ContractCreditCheck(contractResult.Item1.Id, contractOwnerId);
                    if (checkResult != null)
                    {
                        creditCheckAlerts.AddRange(checkResult.Item2);
                    }

                    if (creditCheckAlerts.Any(x => x.Type == AlertType.Error))
                    {
                        aspireFailedResults.Add(Tuple.Create(contractResult.Item1.Id, false));
                    }                    
                }

                //if all aspire opertaion is failed

                var isErrors = creditCheckAlerts.Any(x => x.Type == AlertType.Error) || aspireFailedResults.Any();
                if (isErrors)
                {
                    return new Tuple<ContractDTO, IList<Alert>>(null, creditCheckAlerts);
                }

                //select any of newly created contracts for create a new user in Customer Wallet portal

                var creditReviewStates = _configuration.GetSetting(WebConfigKeys.CREDIT_REVIEW_STATUS_CONFIG_KEY)?.Split(',').Select(s => s.Trim()).ToArray();
                var succededContracts = contractsResultList.Where(r => r.Item1 != null && r.Item1.ContractState >= ContractState.CreditConfirmed
                                                                && creditReviewStates?.Contains(r.Item1?.Details?.Status) != true).Select(r => r.Item1).ToList();
                if (succededContracts != null && succededContracts.Any())
                {
                    var resultAlerts = await _customerWalletService.CreateCustomerByContractList(succededContracts, contractOwnerId);

                    if (resultAlerts.All(x => x.Type != AlertType.Error))
                    {
                        var hasInstallationAddress = succededContracts
                            .FirstOrDefault()
                            .PrimaryCustomer.Locations
                            .FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress) != null;

                        if (succededContracts.Select(x => x.Equipment?.NewEquipment?.FirstOrDefault()).Any(i => i != null) && hasInstallationAddress)
                        {
                            await _mailService.SendHomeImprovementMailToCustomer(succededContracts);

                            foreach (var contract in succededContracts)
                            {
                                if (IsContractUnassignable(contract.Id))
                                {
                                    await _mailService.SendNotifyMailNoDealerAcceptLead(contract);
                                }
                            }
                        }
                    }
                    else
                    {
                        return new Tuple<ContractDTO, IList<Alert>>(null, resultAlerts);
                    }
                }
                else
                {
                    _loggingService.LogWarning($"Customer contract(s) for dealer {contractOwnerId} wasn't approved on Aspire");
                    await _mailService.SendDeclinedConfirmation(newCustomer.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                                                                newCustomer.PrimaryCustomer.FirstName, newCustomer.PrimaryCustomer.LastName);
                }

                //remove all newly created "internal" (unsubmitted to aspire) contracts here
                contractsResultList.Where(r => r.Item1 != null && string.IsNullOrEmpty(r.Item1.Details?.TransactionId)).ForEach(
                    cr =>
                    {
                        _loggingService.LogWarning($"Internal Contract {cr.Item1.Id} is removing from DB");
                        creditCheckAlerts.Add(new Alert
                        {
                            Type = AlertType.Warning,
                            Header = "Internal contract removed",
                            Message = $"Internal contract { cr.Item1.Id } was removed from DB"
                        });
                        var removeAlerts = RemoveContract(cr.Item1.Id, contractOwnerId);
                        if (removeAlerts.Any())
                        {
                            creditCheckAlerts.AddRange(removeAlerts);
                        }
                    });

                //?
                var contractDTO = Mapper.Map<ContractDTO>(succededContracts.FirstOrDefault() ?? contractsResultList?.FirstOrDefault()?.Item1);
                return new Tuple<ContractDTO, IList<Alert>>(contractDTO, creditCheckAlerts);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}]", ex);
                throw;
            }
        }

        private async Task<Tuple<Contract, bool>> InitializeCreating(string contractOwnerId, NewCustomerDTO newCustomer, EquipmentTypeDTO improvmentType = null)
        {
            var contract = _contractRepository.CreateContract(contractOwnerId);

            if (contract != null)
            {
                _unitOfWork.Save();

                var customer = Mapper.Map<Customer>(newCustomer.PrimaryCustomer);

                var contractData = new ContractData
                {
                    PrimaryCustomer = customer,
                    HomeOwners = new List<Customer> { customer },
                    DealerId = contractOwnerId,
                    Id = contract.Id
                };

                if (improvmentType!=null)
                {
                    contractData.Equipment = new EquipmentInfo()
                    {
                        NewEquipment = new List<NewEquipment> { new NewEquipment { Type = improvmentType.Type, Description = improvmentType.Description, EquipmentTypeId = improvmentType.Id} }
                    };
                }

                return await UpdateNewContractForCustomer(contractOwnerId, newCustomer, contractData);
            }

            _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}] with improvment type [{improvmentType}]");

            return new Tuple<Contract, bool>(null, false);
        }

        private bool IsContractUnassignable(int contractId)
        {
            return _contractRepository.IsContractUnassignable(contractId);
        }

        private async Task<Tuple<Contract, bool>> UpdateNewContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer, ContractData contractData)
        {
            var updatedContract = _contractRepository.UpdateContractData(contractData, contractOwnerId);

            if (updatedContract == null)
            {
                return new Tuple<Contract, bool>(null, false);
            }
            updatedContract.IsCreatedByBroker = true;

            if (!string.IsNullOrEmpty(newCustomer.CustomerComment))
            {
                var comment = new Comment
                {
                    ContractId = contractData.Id,
                    IsCustomerComment = true,
                    Text = newCustomer.CustomerComment
                };
                _contractRepository.TryAddComment(comment, contractOwnerId);
            }
            _unitOfWork.Save();

            if (updatedContract.PrimaryCustomer != null)
            {
                await _aspireService.UpdateContractCustomer(updatedContract.Id, contractOwnerId);
            }

            return new Tuple<Contract, bool>(updatedContract, true);
        }

        private IList<Alert> RemoveContract(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                if (_contractRepository.DeleteContract(contractOwnerId, contractId))
                {
                    _unitOfWork.Save();
                }
                else
                {
                    var errorMsg = "Cannot remove contract";
                    alerts.Add(new Alert
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractRemoveFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to remove contract", ex);
                alerts.Add(new Alert
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.DocumentUpdateFailed,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }
    }

}
