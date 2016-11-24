using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
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
        private readonly IAspireService _aspireService;
        private readonly ISignatureService _signatureService;
        private readonly IMailService _mailService;

        public ContractService(IContractRepository contractRepository, IUnitOfWork unitOfWork, 
            IAspireService aspireService, ISignatureService signatureService, IMailService mailService, ILoggingService loggingService)
        {
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _aspireService = aspireService;
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
            var contracts = _contractRepository.GetContracts(contractOwnerId);
            var contractsDTO = Mapper.Map<IList<ContractDTO>>(contracts);
            foreach (var contractDTO in contractsDTO)
            {
                var contract = contracts.FirstOrDefault(x => x.Id == contractDTO.Id);
                if (contract != null) { AftermapComments(contract.Comments, contractDTO.Comments, contractOwnerId); }
            }
            return contractsDTO;
        }

        public IList<ContractDTO> GetContracts(IEnumerable<int> ids, string ownerUserId)
        {
            var contracts = _contractRepository.GetContracts(ids, ownerUserId);
            var contractsDTO = Mapper.Map<IList<ContractDTO>>(contracts);
            foreach (var contractDTO in contractsDTO)
            {
                var contract = contracts.FirstOrDefault(x => x.Id == contractDTO.Id);
                if (contract != null) { AftermapComments(contract.Comments, contractDTO.Comments, ownerUserId); }
            }
            return contractsDTO;
        }

        public ContractDTO GetContract(int contractId, string contractOwnerId)
        {
            var contract = _contractRepository.GetContract(contractId, contractOwnerId);
            var contractDTO = Mapper.Map<ContractDTO>(contract);
            AftermapComments(contract.Comments, contractDTO.Comments, contractOwnerId);
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
                }
                else
                {
                    var errorMsg = $"Cannot find a contract [{contract.Id}] for update. Contract owner: [{contractOwnerId}]";
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

                var alerts = _signatureService.ProcessContract(contractId, contractOwnerId, usersForProcessing.ToArray()).GetAwaiter().GetResult();
                return alerts;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to initiate a digital signature for contract [{contractId}]", ex);
                throw;
            }
        }

        public Tuple<AgreementDocument, IList<Alert>> GetPrintAgreement(int contractId, string contractOwnerId)
        {
            return _signatureService.GetContractAgreement(contractId, contractOwnerId).GetAwaiter().GetResult();
        }

        public Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId)
        {
            //stub for future Aspire request

            TimeSpan creditCheckPause = TimeSpan.FromSeconds(10);

            var checkResult = _aspireService.InitiateCreditCheck(contractId, contractOwnerId).GetAwaiter().GetResult();

            if (checkResult?.Item1 != null)
            {
                var creditAmount = checkResult.Item1.CreditAmount > 0 ? checkResult.Item1.CreditAmount : (decimal?)null;
                var scorecardPoints = checkResult.Item1.ScorecardPoints > 0 ? checkResult.Item1.ScorecardPoints : (int?)null;

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
                        _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditContirmed);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.Declined:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditCheckDeclined);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.MoreInfoRequired:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditContirmed);
                        _unitOfWork.Save();
                        break;
                }
            }

            return checkResult;

            //var creditCheck = new CreditCheckDTO()
            //{
            //    CreditCheckState = CreditCheckState.NotInitiated
            //};
            //var alerts = new List<Alert>();
            //var contract = _contractRepository.GetContract(contractId, contractOwnerId);


            //if (contract != null)
            //{
            //    creditCheck.ContractId = contractId;

            //    switch (contract.ContractState)
            //    {
            //        case ContractState.Started:
            //        case ContractState.CustomerInfoInputted:
            //            alerts.Add(new Alert()
            //            {
            //                Type = AlertType.Error, Header = ErrorConstants.CreditCheckFailed, Message = "Credit check process wasn't initiated"
            //            });
            //            break;
            //        case ContractState.CreditCheckInitiated:
            //            //stub - check time
            //            if ((DateTime.Now - contract.LastUpdateTime) < creditCheckPause)
            //            {
            //                creditCheck.CreditCheckState = CreditCheckState.Initiated;
            //                alerts.Add(new Alert()
            //                {
            //                    Type = AlertType.Information, Header = "Credit check in progress", Message = "Credit check in progress",
            //                });
            //            }
            //            else
            //            {
            //                _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.CreditContirmed);
            //                _unitOfWork.Save();
            //                creditCheck.CreditCheckState = CreditCheckState.Approved;
            //                creditCheck.CreditAmount = 15000;
            //            }
            //            break;
            //        case ContractState.CreditCheckDeclined:
            //            creditCheck.CreditCheckState = CreditCheckState.Declined;
            //            alerts.Add(new Alert()
            //            {
            //                Type = AlertType.Warning, Header = "Credit check declined", Message = "Credit declined",
            //            });
            //            break;
            //        case ContractState.CreditContirmed:
            //        case ContractState.Completed:
            //            alerts.Add(new Alert()
            //            {
            //                Type = AlertType.Information, Header = "Credit approved", Message = "Credit approved",
            //            });
            //            creditCheck.CreditCheckState = CreditCheckState.Approved;
            //            creditCheck.CreditAmount = 15000;
            //            break;
            //        default:
            //            throw new ArgumentOutOfRangeException();
            //    }
            //}
            //else
            //{
            //    var errorMsg = $"Cannot find contract [{contractId}] for get credit check";
            //    alerts.Add(new Alert()
            //    {
            //        Type = AlertType.Error, Header = ErrorConstants.CreditCheckFailed, Message = errorMsg
            //    });
            //    _loggingService.LogError(errorMsg);
            //}
            //return new Tuple<CreditCheckDTO, IList<Alert>>(creditCheck, alerts);
        }

        public IList<Alert> SubmitContract(int contractId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            var aspireAlerts = _aspireService.SubmitDeal(contractId, contractOwnerId).GetAwaiter().GetResult();
            if (aspireAlerts?.Any() ?? false)
            {
                alerts.AddRange(aspireAlerts);
            }

            if (aspireAlerts?.All(ae => ae.Type != AlertType.Error) ?? false)
            {
                var creditCheckRes = _aspireService.InitiateCreditCheck(contractId, contractOwnerId).GetAwaiter().GetResult();
                if (creditCheckRes?.Item2?.Any() ?? false)
                {
                    alerts.AddRange(creditCheckRes.Item2);
                }
            }

            var contract = _contractRepository.UpdateContractState(contractId, contractOwnerId, ContractState.Completed);
            if (contract != null)
            {
                _unitOfWork.Save();
                _loggingService.LogInfo($"Contract [{contractId}] submitted");

                Task.Run(async () => await _mailService.SendSubmitNotification(contractId, contractOwnerId));
                //_mailService.SendSubmitNotification(contractId, contractOwnerId);
            }
            else
            {
                var errorMsg = $"Cannot submit contract [{contractId}]";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error, Header = ErrorConstants.SubmitFailed, Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        public IList<FlowingSummaryItemDTO> GetDealsFlowingSummary(string contractsOwnerId, FlowingSummaryType summaryType)
        {
            IList<FlowingSummaryItemDTO> summary = new List<FlowingSummaryItemDTO>();
            var dealerContracts = _contractRepository.GetContracts(contractsOwnerId);

            if (dealerContracts?.Any() ?? false)
            {
                switch (summaryType)
                {
                    case FlowingSummaryType.Month:
                        var grDaysM = dealerContracts.Where(c => c.CreationTime >= DateTime.Today.AddDays(-DateTime.Today.Day)).GroupBy(c => c.CreationTime.Day);

                        for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                        {
                            var contractsG = grDaysM.FirstOrDefault(g => g.Key == i);
                            decimal totalSum = 0;
                            contractsG?.ForEach(c =>
                            {
                                var totalMp = _contractRepository.GetContractTotalMonthlyPayment(c.Id);
                                totalSum += totalMp * (c.Equipment?.RequestedTerm ?? 0);
                            });
                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = i.ToString(), ItemCount = contractsG?.Count() ?? 0, //grDaysM.Count(g => g.Key == i),
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
                        var grDays = dealerContracts.Where(c => c.CreationTime >= DateTime.Today.AddDays(daysDiff)).GroupBy(c => c.CreationTime.DayOfWeek);

                        for (int i = 0; i < weekDays.Length; i++)
                        {
                            DayOfWeek curDay;
                            Enum.TryParse(weekDays[i], out curDay);

                            var contractsW = grDays.FirstOrDefault(g => g.Key == curDay);
                            decimal totalSum = 0;
                            contractsW?.ForEach(c =>
                            {
                                var totalMp = _contractRepository.GetContractTotalMonthlyPayment(c.Id);
                                totalSum += totalMp * (c.Equipment?.RequestedTerm ?? 0);
                            });

                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = weekDays[i], ItemCount = contractsW?.Count() ?? 0, ItemData = totalSum //rand.Next(1, 100)
                            });
                        }
                        break;
                    case FlowingSummaryType.Year:
                        var months = DateTimeFormatInfo.InvariantInfo.MonthNames;
                        var grMonths = dealerContracts.Where(c => c.CreationTime.Year == DateTime.Today.Year).GroupBy(c => c.CreationTime.Month);

                        for (int i = 0; i < months.Length; i++)
                        {
                            var contractsM = grMonths.FirstOrDefault(g => g.Key == i + 1);
                            decimal totalSum = 0;
                            contractsM?.ForEach(c =>
                            {
                                var totalMp = _contractRepository.GetContractTotalMonthlyPayment(c.Id);
                                totalSum += totalMp * (c.Equipment?.RequestedTerm ?? 0);
                            });

                            summary.Add(new FlowingSummaryItemDTO()
                            {
                                ItemLabel = DateTimeFormatInfo.InvariantInfo.MonthNames[i], ItemCount = contractsM?.Count() ?? 0, ItemData = totalSum
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
                        Type = AlertType.Error, Header = ErrorConstants.EquipmentTypesRetrievalFailed, Message = errorMsg
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
                        Type = AlertType.Error, Header = ErrorConstants.ProvinceTaxRateRetrievalFailed, Message = errorMsg
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
                if (customers?.Any() ?? false)
                {
                    customers.ForEach(c => { _contractRepository.UpdateCustomerData(c.Id, Mapper.Map<IList<Location>>(c.Locations), Mapper.Map<IList<Phone>>(c.Phones), Mapper.Map<IList<Email>>(c.Emails)); });
                }
                _unitOfWork.Save();

                //TODO: update customers on aspire
                if (customers?.Any() ?? false)
                {
                    var contractId = customers.First().ContractId;
                    if (contractId.HasValue)
                    {                        
                        _aspireService.UpdateContractCustomer(contractId.Value, contractOwnerId);
                        //if (aspireAlerts?.Any() ?? false)
                        //{
                        //    alerts.AddRange(aspireAlerts);
                        //}
                    }
                }                
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update customers data", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error, Header = "Failed to update customers data", Message = ex.ToString()
                });
                //throw;
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
                }
                else
                {
                    var errorMsg = "Cannot update contract comment";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error, Header = ErrorConstants.CommentUpdateFailed, Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update contract comment", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error, Header = ErrorConstants.CommentUpdateFailed, Message = ex.ToString()
                });
            }

            return new Tuple<int?, IList<Alert>>(comment?.Id, alerts);
        }

        public IList<Alert> RemoveComment(int commentId, string contractOwnerId)
        {
            var alerts = new List<Alert>();

            try
            {
                if (_contractRepository.TryRemoveComment(commentId, contractOwnerId))
                {
                    _unitOfWork.Save();
                }
                else
                {
                    var errorMsg = "Cannot update contract comment";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error, Header = ErrorConstants.CommentUpdateFailed, Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Failed to update contract comment", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error, Header = ErrorConstants.CommentUpdateFailed, Message = ex.ToString()
                });
            }

            return alerts;
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
    }
}
