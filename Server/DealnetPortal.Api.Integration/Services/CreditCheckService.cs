using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class CreditCheckService : ICreditCheckService
    {
        private readonly IAspireService _aspireService;
        private readonly ILoggingService _loggingService;
        private readonly IContractRepository _contractRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreditCheckService(IAspireService aspireService, IContractRepository contractRepository, IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _aspireService = aspireService;
            _contractRepository = contractRepository;
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
        }

        public Tuple<CreditCheckDTO, IList<Alert>> GetCreditCheckResult(int contractId, string contractOwnerId)
        {
            var checkResult = _aspireService.InitiateCreditCheck(contractId, contractOwnerId).GetAwaiter().GetResult();
            if (checkResult?.Item1 != null)
            {
                var creditAmount = checkResult.Item1.CreditAmount > 0 ? checkResult.Item1.CreditAmount : (decimal?)null;
                var scorecardPoints = checkResult.Item1.ScorecardPoints > 0
                    ? checkResult.Item1.ScorecardPoints
                    : (int?)null;

                if (creditAmount.HasValue || scorecardPoints.HasValue)
                {
                    var contract = _contractRepository.GetContract(contractId, contractOwnerId);
                    _contractRepository.UpdateContractData(new ContractData()
                    {
                        Id = contractId,
                        Details = new ContractDetails()
                        {
                            CreditAmount = creditAmount,
                            ScorecardPoints = scorecardPoints,
                            HouseSize = contract.Details.HouseSize,
                            Notes = contract.Details.Notes
                        }
                    }, contractOwnerId);
                }

                switch (checkResult.Item1.CreditCheckState)
                {
                    case CreditCheckState.Approved:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditConfirmed);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.Declined:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditCheckDeclined);
                        _unitOfWork.Save();
                        break;
                    case CreditCheckState.MoreInfoRequired:
                        _contractRepository.UpdateContractState(contractId, contractOwnerId,
                            ContractState.CreditConfirmed);
                        _unitOfWork.Save();
                        break;
                }
            }

            return checkResult;
        }

        public IList<Alert> InitiateCreditCheck(int contractId, string contractOwnerId)
        {
            try
            {
                var alerts = new List<Alert>();
                var contractState = _contractRepository.GetContractState(contractId, contractOwnerId);

                if (contractState == null)
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
                    if (contractState.Value > ContractState.Started)
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
    }


}
