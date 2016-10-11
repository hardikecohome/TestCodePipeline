using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Linq;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    using Models.Contract.EquipmentInformation;

    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISignatureService _signatureService;

        public ContractService(IContractRepository contractRepository, IUnitOfWork unitOfWork, 
            ISignatureService signatureService, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _signatureService = signatureService;
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
            var contracts = _contractRepository.GetContracts(contractOwnerId);
            var contractsDTO = Mapper.Map<IList<ContractDTO>>(contracts);
            return contractsDTO;
        }

        public IList<Contract> GetContracts(IEnumerable<int> ids, string ownerUserId)
        {
            return _contractRepository.GetContracts(ids, ownerUserId);
        }

        public ContractDTO GetContract(int contractId, string contractOwnerId)
        {
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            return Mapper.Map<ContractDTO>(contract);
        }

        public IList<Alert> UpdateContractClientData(int contractId, string contractOwnerId, IList<LocationDTO> addresses, IList<CustomerDTO> customers)
        {
            throw new NotImplementedException();
        }

        public IList<Alert> UpdateContractData(ContractDataDTO contract, string contractOwnerId)
        {
            try
            {
                var alerts = new List<Alert>();               
                var contractData = Mapper.Map<ContractData>(contract);
                var updatedContract = _contractRepository.UpdateContractData(contractData, contractOwnerId);
                if (updatedContract != null)
                {
                    _unitOfWork.Save();
                    _loggingService.LogInfo($"A contract [{contract.Id}] updated");
                }
                else
                {
                    var errorMsg = $"Cannot find a contract [{contract.Id}] for update";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractUpdateFailed,
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to update a contract [{contract.Id}]", ex);
                throw;
            }
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
                    if (contract.ContractState > ContractState.Started)
                    {
                        _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditCheckInitiated);
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

        public IList<Alert> InitiateDigitalSignature(int contractId, string contractOwnerId, SignatureUser[] signatureUsers)
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

                var alerts = _signatureService.ProcessContract(contractId, contractOwnerId, usersForProcessing.ToArray());
                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initiate a digital signature for contract [{contractId}]", ex);
                throw;
            }
        }

        public Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId)
        {
            //stub for future Aspire request

            TimeSpan creditCheckPause = TimeSpan.FromMinutes(2);

            var creditCheck = new CreditCheckDTO()
            {
                CreditCheckState = CreditCheckState.NotInitiated
            };
            var alerts = new List<Alert>();            
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            if (contract != null)
            {
                creditCheck.ContractId = contractId;                
                if (contract.ContractState < ContractState.CreditCheckInitiated)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.CreditCheckFailed,
                        Message = "Credit check process wasn't initiated"
                    });
                }
                else if (contract.ContractState == ContractState.CreditCheckInitiated)
                {
                    //stub - check time
                    if ((DateTime.Now - contract.LastUpdateTime) < creditCheckPause)
                    {
                        creditCheck.CreditCheckState = CreditCheckState.Initiated;
                        alerts.Add(new Alert()
                        {
                            Type = AlertType.Information,
                            Header = "Credit check in progress",
                            Message = "Credit check in progress",
                        });
                    }
                    else
                    {
                        _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditContirmed);
                        _unitOfWork.Save();
                        creditCheck.CreditCheckState = CreditCheckState.Approved;
                        creditCheck.CreditAmount = 10000;
                    }
                }
                else if (contract.ContractState == ContractState.CreditCheckDeclined)
                {
                    creditCheck.CreditCheckState = CreditCheckState.Declined;
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Warning,
                        Header = "Credit check declined",
                        Message = "Credit declined",
                    });
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Information,
                        Header = "Credit approved",
                        Message = "Credit approved",
                    });
                    creditCheck.CreditCheckState = CreditCheckState.Approved;
                    creditCheck.CreditAmount = 10000;
                }
            }
            else
            {
                var errorMsg = $"Cannot find a contract [{contractId}] for get credit check";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.CreditCheckFailed,
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);                
            }
            return new Tuple<CreditCheckDTO, IList<Alert>>(creditCheck, alerts);
        }

        public IList<Alert> SubmitContract(int contractId, string contractOwnerId)
        {
            throw new NotImplementedException();
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
                        var grDaysM = dealerContracts.Where(
                            c => c.CreationTime >= DateTime.Today.AddDays(-DateTime.Today.Day))
                            .GroupBy(c => c.CreationTime.Day);

                        for (int i = 1; i < DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                        {
                            var contractsG = grDaysM.FirstOrDefault(g => g.Key == i);
                            decimal totalSum = 0;
                            contractsG?.ForEach(c =>
                            {
                                int term = 0;
                                int.TryParse(c.Equipment?.RequestedTerm, out term);
                                totalSum += c.Equipment?.TotalMonthlyPayment ?? 0* term;
                            });
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = i.ToString(),
                                ItemCount = contractsG?.Count() ?? 0,//grDaysM.Count(g => g.Key == i),
                                ItemData = totalSum // rand.Next(1, 100)
                            });
                        }
                        break;
                    case FlowingSummaryType.Week:
                        var weekDays = DateTimeFormatInfo.InvariantInfo.DayNames;
                        DayOfWeek fstWeekDay;
                        int curDayIdx = Array.IndexOf(weekDays, DateTime.Today.DayOfWeek.ToString());
                        Enum.TryParse(weekDays[0], out fstWeekDay);
                        var daysDiff = -curDayIdx;
                        var grDays = dealerContracts.Where(c => c.CreationTime >= DateTime.Today.AddDays(daysDiff))
                            .GroupBy(c => c.CreationTime.DayOfWeek);

                        for (int i = 0; i < weekDays.Length; i++)
                        {
                            DayOfWeek curDay;
                            Enum.TryParse(weekDays[i], out curDay);

                            var contractsW = grDays.FirstOrDefault(g => g.Key == curDay);
                            decimal totalSum = 0;
                            contractsW?.ForEach(c =>
                            {
                                int term = 0;
                                int.TryParse(c.Equipment?.RequestedTerm, out term);
                                totalSum += c.Equipment?.TotalMonthlyPayment ?? 0 * term;
                            });

                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = weekDays[i],
                                ItemCount = contractsW?.Count() ?? 0,
                                ItemData = totalSum//rand.Next(1, 100)
                            });
                        }
                        break;
                    case FlowingSummaryType.Year:
                        var months = DateTimeFormatInfo.InvariantInfo.MonthNames;
                        var grMonths = dealerContracts.Where(c => c.CreationTime.Year == DateTime.Today.Year)
                            .GroupBy(c => c.CreationTime.Month);

                        for (int i = 0; i < months.Length; i++)
                        {
                            var contractsM = grMonths.FirstOrDefault(g => g.Key == i+1);
                            decimal totalSum = 0;
                            contractsM?.ForEach(c =>
                            {
                                int term = 0;
                                int.TryParse(c.Equipment?.RequestedTerm, out term);
                                totalSum += c.Equipment?.TotalMonthlyPayment ?? 0 * term;
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

        public Tuple<IList<EquipmentTypeDTO>, IList<Alert>> GetEquipmentTypes()
        {
            var alerts = new List<Alert>();
            try
            {
                var equipmentTypes = _contractRepository.GetEquipmentTypes();
                var equipmentTypeDtos = Mapper.Map<IList<EquipmentTypeDTO>>(equipmentTypes);
                if (equipmentTypes == null)
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

        public IList<Alert> UpdateCustomers(CustomerDataDTO[] customers)
        {
            var alerts = new List<Alert>();

            try
            {            
                if (customers?.Any() ?? false)
                {
                    customers.ForEach(c =>
                    {
                        _contractRepository.UpdateCustomerData(c.Id, 
                            Mapper.Map<IList<Location>>(c.Locations),
                            Mapper.Map<IList<Phone>>(c.Phones),
                            Mapper.Map<IList<Email>>(c.Emails));
                    });
                }
                _unitOfWork.Save();
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
                //throw;
            }

            return alerts;
        }
    }
}
