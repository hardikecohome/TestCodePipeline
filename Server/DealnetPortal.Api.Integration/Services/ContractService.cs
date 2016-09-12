using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.MemoryMappedFiles;
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
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;

        public ContractService(IContractRepository contractRepository, IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
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

        public ContractDTO GetContract(int contractId)
        {
            var contract = _contractRepository.GetContract(contractId);
            return Mapper.Map<ContractDTO>(contract);
        }

        public IList<Alert> UpdateContractClientData(int contractId, IList<ContractAddressDTO> addresses, IList<CustomerDTO> customers)
        {
            try
            {
                var alerts = new List<Alert>();
                IList<Location> addressesForUpdate = null;
                IList<Customer> customersForUpdate = null;
                if (addresses != null)
                {
                    addressesForUpdate = Mapper.Map<IList<Location>>(addresses);
                }
                if (customers != null)
                {
                    customersForUpdate = Mapper.Map<IList<Customer>>(customers);
                }

                var updatedContract = _contractRepository.UpdateContractClientData(contractId, addressesForUpdate, customersForUpdate);                
                if (updatedContract != null)
                {
                    _unitOfWork.Save();
                    _loggingService.LogInfo($"A contract [{contractId}] updated");
                }
                else
                {
                    var errorMsg = $"Cannot find a contract [{contractId}] for update";
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
                _loggingService.LogError($"Failed to update a contract [{contractId}]", ex);
                throw;
            }
        }

        public IList<Alert> UpdateContractData(ContractDTO contract)
        {
            return UpdateContractClientData(contract.Id, contract.Addresses, contract.Customers);
        }

        public IList<Alert> InitiateCreditCheck(int contractId)
        {
            try
            {
                var alerts = new List<Alert>();
                var contract = _contractRepository.GetContract(contractId);
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
                        _contractRepository.UpdateContractState(contractId, ContractState.CreditCheckInitiated);
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

        public Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId)
        {
            //stub for future Aspire request

            TimeSpan creditCheckPause = TimeSpan.FromMinutes(2);

            var creditCheck = new CreditCheckDTO()
            {
                CreditCheckState = CreditCheckState.NotInitiated
            };
            var alerts = new List<Alert>();            
            var contract = _contractRepository.GetContract(contractId);
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
                        _contractRepository.UpdateContractState(contractId, ContractState.CreditContirmed);
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

        public IList<Alert> SubmitContract(int contractId)
        {
            throw new NotImplementedException();
        }

        public IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId,
            FlowingSummaryType summaryType)
        {
            // Stub for a future deals summary requests

            IList<FlowingSummaryItemDTO> summary = new List<FlowingSummaryItemDTO>();
            var rand = new Random();
            switch (summaryType)
            {
                case FlowingSummaryType.Month:
                    {
                        for (int i = 1; i < DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                        {
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = i.ToString(),
                                ItemData = rand.Next(1, 100)
                            });                            
                        }
                        break;
                    }
                case FlowingSummaryType.Week:
                    {
                        foreach (var day in DateTimeFormatInfo.CurrentInfo.DayNames)
                        {
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = day,
                                ItemData = rand.Next(1, 100)
                            });
                        }
                        break;
                    }
                case FlowingSummaryType.Year:
                    {
                        foreach (var month in DateTimeFormatInfo.CurrentInfo.MonthNames)
                        {
                            if (string.IsNullOrEmpty(month))
                            {
                                continue;
                            }
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = month,
                                ItemData = rand.Next(1, 100) 
                            });
                        }
                        break;
                    }
            }

            return summary;
        }
    }
}
