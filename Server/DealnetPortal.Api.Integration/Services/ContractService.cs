using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Practices.ObjectBuilder2;

using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    using Models.Contract.EquipmentInformation;

    public partial class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAspireService _aspireService;
        private readonly IAspireStorageReader _aspireStorageReader;
        private readonly ICustomerWalletService _customerWalletService;
        private readonly ISignatureService _signatureService;
        private readonly IMailService _mailService;

        public ContractService(
            IContractRepository contractRepository, 
            IUnitOfWork unitOfWork, 
            IAspireService aspireService,
            IAspireStorageReader aspireStorageReader, 
            ICustomerWalletService customerWalletService,
            ISignatureService signatureService, 
            IMailService mailService, 
            ILoggingService loggingService, IDealerRepository dealerRepository)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _dealerRepository = dealerRepository;
            _unitOfWork = unitOfWork;
            _aspireService = aspireService;
            _aspireStorageReader = aspireStorageReader;
            _customerWalletService = customerWalletService;
            _signatureService = signatureService;
            _mailService = mailService;
        }

        public ContractDTO CreateContract(string contractOwnerId)
        {
            try
            {
                var newContract = _contractRepository.CreateContract(contractOwnerId);
                if (newContract != null)
                {
                    _unitOfWork.Save();
                    var contractDTO = Mapper.Map<ContractDTO>(newContract);
                    _loggingService.LogInfo($"A new contract [{newContract.Id}] created by user [{contractOwnerId}]");
                    return contractDTO;
                }
                else
                {
                    _loggingService.LogError($"Failed to create a new contract for a user [{contractOwnerId}]");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to create a new contract for a user [{contractOwnerId}]", ex);
                throw;
            }
        }

        public IList<ContractDTO> GetContracts(string contractOwnerId)
        {
            var contractDTOs = new List<ContractDTO>();

            var aspireDeals = GetAspireDealsForDealer(contractOwnerId);

            if (aspireDeals?.Any() ?? false)
            {
                // update dealer-sub dealers hierarchy
                var transactionIds = aspireDeals.Select(d => d.Details?.TransactionId).ToArray();
                var updatedDealers = _contractRepository.UpdateSubDealersHierarchyByRelatedTransactions(transactionIds,
                    contractOwnerId);
                if (updatedDealers > 0)
                {
                    try
                    {
                        _loggingService.LogInfo(
                            $"Updating relashionships for {updatedDealers} SubDealers or Sales Agents");
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("Cannot update Sub-dealers and Sales agents hierarchy", ex);
                    }
                }
            }

            var contracts = _contractRepository.GetContracts(contractOwnerId);

            if (aspireDeals?.Any() ?? false)
            {
                var isContractsUpdated = UpdateContractsByAspireDeals(contracts, aspireDeals);

                var unlinkedDeals = aspireDeals.Where(ad => ad.Details?.TransactionId != null &&
                                                            contracts.All(
                                                                c =>
                                                                    (!c.Details?.TransactionId?.Contains(
                                                                        ad.Details.TransactionId) ?? true))).ToList();
                if (unlinkedDeals.Any())
                {
                    contractDTOs.AddRange(unlinkedDeals);
                }
                if (isContractsUpdated)
                {
                    try
                    {
                        _unitOfWork.Save();
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError("Cannot update Aspire deals status", ex);
                    }
                }
            }

            var mappedContracts = Mapper.Map<IList<ContractDTO>>(contracts);
            AftermapContracts(contracts, mappedContracts, contractOwnerId);
            contractDTOs.AddRange(mappedContracts);

            return contractDTOs;
        }

        public int GetCustomersContractsCount(string contractOwnerId)
        {
            return _contractRepository.GetNewlyCreatedCustomersContractsCount(contractOwnerId);
        }

        public IList<ContractDTO> GetContracts(IEnumerable<int> ids, string ownerUserId)
        {
            var contracts = _contractRepository.GetContracts(ids, ownerUserId);
            var contractDTOs = Mapper.Map<IList<ContractDTO>>(contracts);
            AftermapContracts(contracts, contractDTOs, ownerUserId);
            return contractDTOs;
        }

        public ContractDTO GetContract(int contractId, string contractOwnerId)
        {
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            var contractDTO = Mapper.Map<ContractDTO>(contract);
            if (contractDTO != null)
            {
                AftermapNewEquipment(contractDTO.Equipment?.NewEquipment, _contractRepository.GetEquipmentTypes());
                AftermapComments(contract.Comments, contractDTO.Comments, contractOwnerId);
            }
            return contractDTO;
        }

        public IList<Alert> UpdateContractData(ContractDataDTO contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            try
            {
                var contractData = Mapper.Map<ContractData>(contract);
                var updatedContract = _contractRepository.UpdateContractData(contractData, contractOwnerId);
                if (updatedContract != null)
                {
                    _unitOfWork.Save();
                    _loggingService.LogInfo($"A contract [{contract.Id}] updated");

                    //update customers on aspire
                    if (contract.PrimaryCustomer != null || contract.SecondaryCustomers != null)
                    {
                        var aspireAlerts =
                            _aspireService.UpdateContractCustomer(contract.Id, contractOwnerId);
                        //if (aspireAlerts?.Any() ?? false)
                        //{
                        //    alerts.AddRange(aspireAlerts);
                        //}
                    }
                    if (updatedContract.ContractState == ContractState.Completed)
                    {
                        var contractDTO = Mapper.Map<ContractDTO>(updatedContract);
                        Task.Run(
                            async () =>
                                await
                                    _mailService.SendContractChangeNotification(contractDTO,
                                        updatedContract.Dealer.Email));
                    }
                }
                else
                {
                    var errorMsg =
                        $"Cannot find a contract [{contract.Id}] for update. Contract owner: [{contractOwnerId}]";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractUpdateFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to update a contract [{contract.Id}]", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractUpdateFailed,
                    Message = $"Failed to update a contract [{contract.Id}]"
                });
            }
            return alerts;
        }

        public IList<Alert> NotifyContractEdit(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                //Remove newly created by customer mark, if contract is opened for edit
                try
                {
                    contract.IsNewlyCreated = false;
                    _unitOfWork.Save();
                }
                catch (Exception ex)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractUpdateFailed,
                        Code = ErrorCodes.FailedToUpdateContract,
                        Message = $"Cannot update contract [{contractId}]"
                    });
                    _loggingService.LogError($"Cannot update contract [{contractId}]", ex);
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.CantGetContractFromDb,
                    Header = "Cannot find contract",
                    Message = $"Cannot find contract [{contractId}] for update"
                });
            }
            return alerts;
        }

        public IList<Alert> InitiateCreditCheck(int contractId, string contractOwnerId)
        {
            try
            {
                var alerts = new List<Alert>();
                var contract = _contractRepository.GetContract(contractId, contractOwnerId);

                if (contract == null)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.CreditCheckFailed,
                        Message = "Cannot find a contract [{contractId}] for initiate credit check"
                    });
                }
                else
                {
                    //TODO: credit check ?
                    if (contract.ContractState > ContractState.Started)
                    {
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditCheckInitiated);
                        _unitOfWork.Save();
                        _loggingService.LogInfo($"Initiated credit check for contract [{contractId}]");
                    }
                    else
                    {
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Error,
                            Header = ErrorConstants.CreditCheckFailed,
                            Message = "Cannot initiate credit check for contract with lack of customers information"
                        });
                    }
                }

                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initiate a credit check for contract [{contractId}]", ex);
                throw;
            }
        }

        public IList<Alert> InitiateDigitalSignature(int contractId, string contractOwnerId,
            SignatureUser[] signatureUsers)
        {
            try
            {
                List<SignatureUser> usersForProcessing = new List<SignatureUser>();
                var contract = _contractRepository.GetContract(contractId, contractOwnerId);
                var homeOwner = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.HomeOwner);
                if (homeOwner != null)
                {
                    homeOwner.FirstName = contract?.PrimaryCustomer?.FirstName;
                    homeOwner.LastName = contract?.PrimaryCustomer?.LastName;
                    usersForProcessing.Add(homeOwner);
                }

                var coCustomers = signatureUsers.Where(u => u.Role == SignatureRole.AdditionalApplicant).ToList();
                if (coCustomers.Any())
                {
                    int i = 0;
                    contract?.SecondaryCustomers?.ForEach(cc =>
                    {
                        if (i < coCustomers.Count && !string.IsNullOrEmpty(coCustomers[i].EmailAddress))
                        {
                            coCustomers[i].FirstName = cc.FirstName;
                            coCustomers[i].LastName = cc.LastName;
                            usersForProcessing.Add(coCustomers[i]);
                            i++;
                        }
                    });
                }

                var dealerUser = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.Dealer);
                if (dealerUser != null)
                {
                    var dealer = _contractRepository.GetDealer(contractOwnerId);
                    if (dealer != null)
                    {
                        dealerUser.LastName = dealer.UserName;
                        usersForProcessing.Add(dealerUser);
                    }
                }

                var alerts =
                    _signatureService.ProcessContract(contractId, contractOwnerId, usersForProcessing.ToArray())
                        .GetAwaiter()
                        .GetResult();
                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initiate a digital signature for contract [{contractId}]", ex);
                throw;
            }
        }

        public Tuple<bool, IList<Alert>> CheckPrintAgreementAvailable(int contractId, int documentTypeId,
            string contractOwnerId)
        {
            return
                _signatureService.CheckPrintAgreementAvailable(contractId, documentTypeId, contractOwnerId)
                    .GetAwaiter()
                    .GetResult();
        }

        public Tuple<AgreementDocument, IList<Alert>> GetPrintAgreement(int contractId, string contractOwnerId)
        {
            return _signatureService.GetPrintAgreement(contractId, contractOwnerId).GetAwaiter().GetResult();
        }

        public Tuple<AgreementDocument, IList<Alert>> GetInstallCertificate(int contractId, string contractOwnerId)
        {
            return _signatureService.GetInstallCertificate(contractId, contractOwnerId).GetAwaiter().GetResult();
        }

        public IList<Alert> UpdateInstallationData(InstallationCertificateDataDTO installationCertificateData,
            string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                var contract = _contractRepository.GetContract(installationCertificateData.ContractId, contractOwnerId);

                if (contract != null)
                {
                    if (contract.Equipment != null)
                    {
                        if (!string.IsNullOrEmpty(installationCertificateData.InstallerFirstName))
                        {
                            contract.Equipment.InstallerFirstName = installationCertificateData.InstallerFirstName;
                        }
                        if (!string.IsNullOrEmpty(installationCertificateData.InstallerLastName))
                        {
                            contract.Equipment.InstallerLastName = installationCertificateData.InstallerLastName;
                        }
                        if (installationCertificateData.InstallationDate.HasValue)
                        {
                            contract.Equipment.InstallationDate = installationCertificateData.InstallationDate;
                        }
                        if (installationCertificateData.InstalledEquipments?.Any() ?? false)
                        {
                            installationCertificateData.InstalledEquipments.ForEach(ie =>
                            {
                                var eq = contract.Equipment.NewEquipment.FirstOrDefault(e => e.Id == ie.Id);
                                if (eq != null)
                                {
                                    if (!string.IsNullOrEmpty(ie.Model))
                                    {
                                        eq.InstalledModel = ie.Model;
                                    }
                                    if (!string.IsNullOrEmpty(ie.SerialNumber))
                                    {
                                        eq.InstalledSerialNumber = ie.SerialNumber;
                                    }
                                }
                            });
                        }
                    }
                    _unitOfWork.Save();
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.CreditCheckFailed,
                        Message = "Cannot find a contract [{contractId}] for initiate credit check"
                    });
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(
                    $"Cannot update installation data for contract {installationCertificateData.ContractId}", ex);
            }
            return alerts;
        }

        public Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId)
        {
            var checkResult = _aspireService.InitiateCreditCheck(contractId, contractOwnerId).GetAwaiter().GetResult();
            if (checkResult?.Item1 != null)
            {
                var creditAmount = checkResult.Item1.CreditAmount > 0 ? checkResult.Item1.CreditAmount : (decimal?) null;
                var scorecardPoints = checkResult.Item1.ScorecardPoints > 0
                    ? checkResult.Item1.ScorecardPoints
                    : (int?) null;

                if (creditAmount.HasValue || scorecardPoints.HasValue)
                {
                    _contractRepository.UpdateContractData(new ContractData()
                    {
                        Id = contractId,
                        Details = new ContractDetails()
                        {
                            CreditAmount = creditAmount,
                            ScorecardPoints = scorecardPoints
                        }
                    }, contractOwnerId);
                }

                switch (checkResult.Item1.CreditCheckState)
                {
                    case CreditCheckState.Approved:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditContirmed);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.Declined:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditCheckDeclined);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.MoreInfoRequired:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditContirmed);
                        _unitOfWork.Save();
                        break;
                }
            }

            return checkResult;
        }

        public Tuple<CreditCheckDTO, IList<Alert>> SubmitContract(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            CreditCheckDTO creditCheck = null;

            var aspireAlerts = _aspireService.SubmitDeal(contractId, contractOwnerId).GetAwaiter().GetResult();
            if (aspireAlerts?.Any() ?? false)
            {
                alerts.AddRange(aspireAlerts);
            }

            if (aspireAlerts?.All(ae => ae.Type != AlertType.Error) ?? false)
            {
                var creditCheckRes =
                    _aspireService.InitiateCreditCheck(contractId, contractOwnerId).GetAwaiter().GetResult();
                if (creditCheckRes?.Item2?.Any() ?? false)
                {
                    alerts.AddRange(creditCheckRes.Item2);
                }
                creditCheck = creditCheckRes?.Item1;
                Contract contract = null;
                switch (creditCheckRes?.Item1.CreditCheckState)
                {
                    case CreditCheckState.Declined:
                        contract = _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditCheckDeclined);
                        break;
                    default:
                        contract = _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.Completed);
                        break;
                }
                //var contract = _contractRepository.UpdateContractState(contractId, contractOwnerId,
                //    ContractState.Completed);
                if (contract != null)
                {
                    _unitOfWork.Save();
                    var submitState = creditCheckRes.Item1.CreditCheckState == CreditCheckState.Declined
                        ? "declined"
                        : "submitted";
                    _loggingService.LogInfo($"Contract [{contractId}] {submitState}");

                    var contractDTO = Mapper.Map<ContractDTO>(contract);
                    Task.Run(
                        async () =>
                            await
                                _mailService.SendContractSubmitNotification(contractDTO, contract.Dealer.Email,
                                    creditCheckRes.Item1.CreditCheckState != CreditCheckState.Declined));
                    //_mailService.SendContractSubmitNotification(contractId, contractOwnerId);
                }
                else
                {
                    var errorMsg = $"Cannot submit contract [{contractId}]";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.SubmitFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            return new Tuple<CreditCheckDTO, IList<Alert>>(creditCheck, alerts);
        }

        public IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId,
            FlowingSummaryType summaryType)
        {
            IList<FlowingSummaryItemDTO> summary = new List<FlowingSummaryItemDTO>();
            var dealerContracts = _contractRepository.GetContracts(contractsOwnerId);

            if (dealerContracts?.Any() ?? false)
            {
                switch (summaryType)
                {
                    case FlowingSummaryType.Month:
                        var grDaysM =
                            dealerContracts.Where(c => c.CreationTime >= DateTime.Today.AddDays(-DateTime.Today.Day))
                                .GroupBy(c => c.CreationTime.Day)
                                .ToList();

                        for (var i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                        {
                            var contractsG = grDaysM.FirstOrDefault(g => g.Key == i);
                            double totalSum = 0;
                            contractsG?.ForEach(c =>
                            {
                                //var totalMp = _contractRepository.GetContractPaymentsSummary(c.Id);
                                //totalSum += totalMp?.TotalAllMonthlyPayment ?? 0;
                                totalSum += c.Equipment?.ValueOfDeal ?? 0;
                            });
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = i.ToString(),
                                ItemCount = contractsG?.Count() ?? 0, //grDaysM.Count(g => g.Key == i),
                                ItemData = totalSum // rand.Next(1, 100)
                            });
                        }
                        break;
                    case FlowingSummaryType.Week:
                        var weekDays = DateTimeFormatInfo.CurrentInfo.DayNames;
                        var curDayIdx = Array.IndexOf(weekDays,
                            Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetDayName(DateTime.Today.DayOfWeek));
                        var daysDiff = -curDayIdx;
                        var grDays =
                            dealerContracts.Where(c => c.CreationTime >= DateTime.Today.AddDays(daysDiff))
                                .GroupBy(
                                    c =>
                                        Thread.CurrentThread.CurrentCulture.DateTimeFormat.GetDayName(
                                            c.CreationTime.DayOfWeek))
                                .ToList();

                        for (int i = 0; i < weekDays.Length; i++)
                        {
                            var contractsW = grDays.FirstOrDefault(g => g.Key == weekDays[i]);
                            double totalSum = 0;
                            contractsW?.ForEach(c =>
                            {
                                //var totalMp = _contractRepository.GetContractPaymentsSummary(c.Id);
                                //totalSum += totalMp?.TotalAllMonthlyPayment ?? 0;
                                totalSum += c.Equipment?.ValueOfDeal ?? 0;
                            });

                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = weekDays[i],
                                ItemCount = contractsW?.Count() ?? 0,
                                ItemData = totalSum //rand.Next(1, 100)
                            });
                        }
                        break;
                    case FlowingSummaryType.Year:
                        var months = DateTimeFormatInfo.CurrentInfo.MonthNames;
                        var grMonths =
                            dealerContracts.Where(c => c.CreationTime.Year == DateTime.Today.Year)
                                .GroupBy(c => c.CreationTime.Month)
                                .ToList();

                        for (int i = 0; i < months.Length; i++)
                        {
                            var contractsM = grMonths.FirstOrDefault(g => g.Key == i + 1);
                            double totalSum = 0;
                            contractsM?.ForEach(c =>
                            {
                                //var totalMp = _contractRepository.GetContractPaymentsSummary(c.Id);
                                //totalSum += totalMp?.TotalAllMonthlyPayment ?? 0;
                                totalSum += c.Equipment?.ValueOfDeal ?? 0;
                            });

                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = DateTimeFormatInfo.CurrentInfo.MonthNames[i],
                                ItemCount = contractsM?.Count() ?? 0,
                                ItemData = totalSum
                            });
                        }
                        break;
                }
            }

            return summary;
        }

        public Tuple<IList<EquipmentTypeDTO>, IList<Alert>> GetEquipmentTypes(string dealerId)
        {
            var alerts = new List<Alert>();
            try
            {
                var dealerProfile = _dealerRepository.GetDealerProfile(dealerId);
                IList<EquipmentType> equipmentTypes;
                if (dealerProfile != null)
                {
                    equipmentTypes = dealerProfile.Equipments.Select(x=>x.Equipment).ToList();
                }
                else
                {
                    equipmentTypes = _contractRepository.GetEquipmentTypes();
                }
                
                var equipmentTypeDtos = Mapper.Map<IList<EquipmentTypeDTO>>(equipmentTypes);
                if (!equipmentTypes.Any())
                {
                    var errorMsg = "Cannot retrieve Equipment Types";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.EquipmentTypesRetrievalFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
                return new Tuple<IList<EquipmentTypeDTO>, IList<Alert>>(equipmentTypeDtos, alerts);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to retrieve Equipment Types", ex);
                throw;
            }
        }

        public Tuple<ProvinceTaxRateDTO, IList<Alert>> GetProvinceTaxRate(string province)
        {
            var alerts = new List<Alert>();
            try
            {
                var provinceTaxRate = _contractRepository.GetProvinceTaxRate(province);
                var provinceTaxRateDto = Mapper.Map<ProvinceTaxRateDTO>(provinceTaxRate);
                if (provinceTaxRate == null)
                {
                    var errorMsg = "Cannot retrieve Province Tax Rate";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ProvinceTaxRateRetrievalFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
                return new Tuple<ProvinceTaxRateDTO, IList<Alert>>(provinceTaxRateDto, alerts);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to retrieve Province Tax Rate", ex);
                throw;
            }
        }

        public CustomerDTO GetCustomer(int customerId)
        {
            var customer = _contractRepository.GetCustomer(customerId);
            return Mapper.Map<CustomerDTO>(customer);
        }

        public IList<Alert> UpdateCustomers(CustomerDataDTO[] customers, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                // update only new customers for declined deals
                if (customers?.Any() ?? false)
                {
                    var contractId = customers.FirstOrDefault(c => c.ContractId.HasValue)?.ContractId;
                    var contract = contractId.HasValue
                        ? _contractRepository.GetContractAsUntracked(contractId.Value, contractOwnerId)
                        : null;

                    var customersUpdated = false;

                    customers.ForEach(c =>
                    {
                        if (contract == null || contract.WasDeclined != true ||
                            (contract.InitialCustomers?.All(ic => ic.Id != c.Id) ?? false))
                        {
                            _contractRepository.UpdateCustomerData(c.Id, Mapper.Map<Customer>(c.CustomerInfo),
                                Mapper.Map<IList<Location>>(c.Locations), Mapper.Map<IList<Phone>>(c.Phones),
                                Mapper.Map<IList<Email>>(c.Emails));
                            customersUpdated = true;
                        }
                    });

                    if (customersUpdated == true)
                    {
                        _unitOfWork.Save();

                        // get latest contract changes
                        if (contractId.HasValue)
                        {
                            contract = _contractRepository.GetContractAsUntracked(contractId.Value, contractOwnerId);
                        }
                        // update customers on aspire
                        if (contract != null)
                        {
                            if (contract.ContractState == ContractState.Completed)
                            {
                                var contractDTO = Mapper.Map<ContractDTO>(contract);
                                Task.Run(
                                    async () =>
                                        await
                                            _mailService.SendContractChangeNotification(contractDTO,
                                                contract.Dealer.Email));
                            }
                            _aspireService.UpdateContractCustomer(contractId.Value, contractOwnerId);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update customers data", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Failed to update customers data",
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        public Tuple<int?, IList<Alert>> AddComment(CommentDTO commentDTO, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            Comment comment = null;

            try
            {
                comment = _contractRepository.TryAddComment(Mapper.Map<Comment>(commentDTO), contractOwnerId);
                if (comment != null)
                {
                    _unitOfWork.Save();
                    if (comment.ContractId.HasValue)
                    {
                        var contract = _contractRepository.GetContractAsUntracked(comment.ContractId.Value,
                            contractOwnerId);
                        var contractDTO = Mapper.Map<ContractDTO>(contract);
                        Task.Run(
                            async () =>
                                await _mailService.SendContractChangeNotification(contractDTO, contract.Dealer.Email));
                    }
                }
                else
                {
                    var errorMsg = "Cannot update contract comment";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.CommentUpdateFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update contract comment", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.CommentUpdateFailed,
                    Message = ex.ToString()
                });
            }

            return new Tuple<int?, IList<Alert>>(comment?.Id, alerts);
        }

        public IList<Alert> RemoveComment(int commentId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                var removedCommentContractId = _contractRepository.RemoveComment(commentId, contractOwnerId);
                if (removedCommentContractId != null)
                {
                    _unitOfWork.Save();
                    var contract = _contractRepository.GetContractAsUntracked(removedCommentContractId.Value,
                        contractOwnerId);
                    var contractDTO = Mapper.Map<ContractDTO>(contract);
                    Task.Run(
                        async () =>
                            await _mailService.SendContractChangeNotification(contractDTO, contract.Dealer.Email));
                }
                else
                {
                    var errorMsg = "Cannot update contract comment";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.CommentUpdateFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update contract comment", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.CommentUpdateFailed,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        public IList<ContractDTO> GetDealerLeads(string userId)
        {
            var contractDTOs = new List<ContractDTO>();            
            // temporary using a flag IsCreatedByBroker
            var contracts = _contractRepository.GetDealerLeads(userId);            
            var mappedContracts = Mapper.Map<IList<ContractDTO>>(contracts);
            AftermapContracts(contracts, mappedContracts, userId);
            contractDTOs.AddRange(mappedContracts);
            return contractDTOs;
        }

        public IList<ContractDTO> GetCreatedContracts(string userId)
        {
            var contracts = _contractRepository.GetContractsCreatedByUser(userId);
            var contractDTOs = Mapper.Map<IList<ContractDTO>>(contracts);
            AftermapContracts(contracts, contractDTOs, userId);
            return contractDTOs;
        }

        private void AftermapContracts(IList<Contract> contracts, IList<ContractDTO> contractDTOs, string ownerUserId)
        {
            var equipmentTypes = _contractRepository.GetEquipmentTypes();
            foreach (var contractDTO in contractDTOs)
            {
                AftermapNewEquipment(contractDTO.Equipment?.NewEquipment, equipmentTypes);
                var contract = contracts.FirstOrDefault(x => x.Id == contractDTO.Id);
                if (contract != null) { AftermapComments(contract.Comments, contractDTO.Comments, ownerUserId); }
            }
        }

        private void AftermapNewEquipment(IList<NewEquipmentDTO> equipment, IList<EquipmentType> equipmentTypes)
        {
            equipment?.ForEach(eq => eq.TypeDescription = ResourceHelper.GetGlobalStringResource(equipmentTypes.FirstOrDefault(eqt => eqt.Type == eq.Type)?.DescriptionResource));
        }

        private void AftermapComments(IEnumerable<Comment> src, IEnumerable<CommentDTO> dest, string contractOwnerId)
        {
            var srcComments = src.ToArray();
            foreach (var destComment in dest)
            {
                var scrComment = srcComments.FirstOrDefault(x => x.Id == destComment.Id);
                if (scrComment == null)
                {
                    continue;
                }
                destComment.IsOwn = scrComment.DealerId == contractOwnerId;
                if (destComment.Replies.Any())
                {
                    AftermapComments(scrComment.Replies, destComment.Replies, contractOwnerId);
                }
            }
        }

        private IList<ContractDTO> GetAspireDealsForDealer(string contractOwnerId)
        {
            var user = _contractRepository.GetDealer(contractOwnerId);
            if (user != null)
            {
                try
                {
                    //var deals = _aspireStorageService.GetDealerDeals(user.DisplayName);
                    var deals = Mapper.Map<IList<ContractDTO>>(_aspireStorageReader.GetDealerDeals(user.UserName));
                    if (deals?.Any() ?? false)
                    {
                        //skip deals that already in DB                        
                        var equipments = _contractRepository.GetEquipmentTypes();
                        if (equipments?.Any() ?? false)
                        {
                            deals.ForEach(d =>
                            {
                                var eqType = d.Equipment?.NewEquipment?.FirstOrDefault()?.Type;
                                if (!string.IsNullOrEmpty(eqType))
                                {
                                    var equipment = equipments.FirstOrDefault(eq => eq.Description == eqType);
                                    if (equipment != null)
                                    {
                                        d.Equipment.NewEquipment.FirstOrDefault().Type = equipment.Type;
                                        d.Equipment.NewEquipment.FirstOrDefault().TypeDescription =
                                            ResourceHelper.GetGlobalStringResource(equipment.Description);
                                    }
                                    else
                                    {
                                        d.Equipment.NewEquipment.FirstOrDefault().TypeDescription = eqType;
                                    }                                                                                                                
                                }
                            });
                        }
                    }
                    return deals;
                }                                
                catch (Exception ex)
                {
                    _loggingService.LogError($"Error occured during get deals from aspire", ex);
                }
            }
            else
            {
                _loggingService.LogError($"Cannot get a dealer {contractOwnerId}");
            }
            return null;
        }

        public Tuple<int?, IList<Alert>> AddDocumentToContract(ContractDocumentDTO document, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            ContractDocument doc = null;
            try
            {
                doc = _contractRepository.AddDocumentToContract(document.ContractId, Mapper.Map<ContractDocument>(document),
                    contractOwnerId);
                _unitOfWork.Save();

                //run aspire upload async
                _aspireService.UploadDocument(document.ContractId, document, contractOwnerId);
                //var aspireAlerts = _aspireService.UploadDocument(document.ContractId, document, contractOwnerId).GetAwaiter().GetResult();
                //if (aspireAlerts?.Any() ?? false)
                //{
                //    alerts.AddRange(aspireAlerts);
                //}
                var contract = _contractRepository.GetContractAsUntracked(doc.ContractId, contractOwnerId);
                var contractDTO = Mapper.Map<ContractDTO>(contract);
                Task.Run(async () => await _mailService.SendContractChangeNotification(contractDTO, contract.Dealer.Email));
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to add document to contract", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error, Header = "Failed to add document to contract", Message = ex.ToString()
                });
            }
            return new Tuple<int?, IList<Alert>>(doc?.Id, alerts); ;
        }

        public IList<Alert> RemoveContractDocument(int documentId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                if (_contractRepository.TryRemoveContractDocument(documentId, contractOwnerId))
                {
                    _unitOfWork.Save();
                }
                else
                {
                    var errorMsg = "Cannot remove contract document";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.DocumentUpdateFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to remove contract document", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.DocumentUpdateFailed,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        public async Task<IList<Alert>> SubmitAllDocumentsUploaded(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            try
            {
                //run aspire upload async
                var aspireAlerts =  await _aspireService.SubmitAllDocumentsUploaded(contractId, contractOwnerId);
                //var aspireAlerts = _aspireService.UploadDocument(document.ContractId, document, contractOwnerId).GetAwaiter().GetResult();
                if (aspireAlerts?.Any() ?? false)
                {
                    alerts.AddRange(aspireAlerts);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to submit All Documents Uploaded Request", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Failed to submit All Documents Uploaded Request",
                    Message = ex.ToString()
                });
            }
            return new List<Alert>(alerts);
        }

        private bool UpdateContractsByAspireDeals(IList<Contract> contractsForUpdate, IList<ContractDTO> aspireDeals)
        {
            bool isChanged = false;
            foreach (var aspireDeal in aspireDeals)
            {
                if (aspireDeal.Details?.TransactionId == null)
                {
                    continue;
                }
                var contract =
                    contractsForUpdate.FirstOrDefault(
                        c => (c.Details?.TransactionId?.Contains(aspireDeal.Details.TransactionId) ?? false));
                if (contract != null)
                {
                    if (contract.Details.Status != aspireDeal.Details.Status)
                    {
                        contract.Details.Status = aspireDeal.Details.Status;
                        contract.LastUpdateTime = DateTime.Now;
                        isChanged = true;                                                
                    }
                    //update contract state in any case
                    UpdateContractState(contract);
                }                
            }
            return isChanged;
        }

        /// <summary>
        /// Logic for update internal contract state by Aspire state
        /// </summary>
        /// <param name="contract"></param>
        private void UpdateContractState(Contract contract)
        {
            var aspireStatus = _contractRepository.GetAspireStatus(contract.Details?.Status);
            if (aspireStatus != null)
            {
                if (!aspireStatus.Interpretation.HasFlag(AspireStatusInterpretation.SentToAudit) &&
                    contract.ContractState == ContractState.SentToAudit)
                {
                    contract.ContractState = ContractState.Completed;
                    contract.LastUpdateTime = DateTime.Now;
                }
                else if (aspireStatus.Interpretation.HasFlag(AspireStatusInterpretation.SentToAudit) &&
                         contract.ContractState != ContractState.SentToAudit)
                {
                    contract.ContractState = ContractState.SentToAudit;
                    contract.LastUpdateTime = DateTime.Now;
                }
            }
            else
            {
                // if current status is SentToAudit reset it to Completed
                if (contract.ContractState == ContractState.SentToAudit)
                {
                    contract.ContractState = ContractState.Completed;
                    contract.LastUpdateTime = DateTime.Now;
                }
            }
        }

        public IList<Alert> RemoveContract(int contractId, string contractOwnerId)
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
                    alerts.Add(new Alert()
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
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.DocumentUpdateFailed,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        public async Task<IList<Alert>> AssignContract(int contractId, string newContractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                var updatedContract = _contractRepository.AssignContract(contractId, newContractOwnerId);
                if (updatedContract != null)
                {
                    _unitOfWork.Save();
                    var dealer = Mapper.Map<DealerDTO>(_aspireStorageReader.GetDealerInfo(updatedContract.Dealer.UserName));
                    var nowaitSend = _mailService.SendCustomerDealerAcceptLead(updatedContract, dealer);
                    var nowaitAspireUpdate = _aspireService.UpdateContractCustomer(contractId, newContractOwnerId);
                }
                else
                {
                    var errorMsg = "Cannot reasign contract";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractUpdateFailed,
                        Message = errorMsg
                    });

                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to remove contract", ex);
                alerts.Add(new Alert()
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
