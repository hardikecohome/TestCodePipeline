using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public partial class ContractService
    {
        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer)
        {
            try
            {
                var contractsResultList = new List<Tuple<Contract, bool>>();

                if (newCustomer.HomeImprovementTypes != null && newCustomer.HomeImprovementTypes.Any())
                {
                    foreach (var improvmentType in newCustomer.HomeImprovementTypes)
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
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractCreateFailed,
                        Message = errorMsg
                    });
                    return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
                }

                //todo: maybe it's better to try few times initiate contract check for unseccessful contracts
                var aspireFailedResults = new List<Tuple<int, bool>>();
                var creditCheckAlerts = new List<Alert>();

                foreach (var contractResult in contractsResultList.Where(x => x.Item1 != null).ToList())
                {
                    var initAlerts = InitiateCreditCheck(contractResult.Item1.Id, contractOwnerId);

                    if (initAlerts?.Any() ?? false)
                    {
                        creditCheckAlerts.AddRange(initAlerts);
                    }

                    var checkResult = GetCreditCheckResult(contractResult.Item1.Id, contractOwnerId);
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
                if (creditCheckAlerts.Any(x => x.Type == AlertType.Error) || aspireFailedResults.Any())
                {
                    return new Tuple<ContractDTO, IList<Alert>>(null, creditCheckAlerts);
                }

                var contractDTO = Mapper.Map<ContractDTO>(contractsResultList.First().Item1);

                return new Tuple<ContractDTO, IList<Alert>>(contractDTO, creditCheckAlerts);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}]", ex);
                throw;
            }
        }

        private async Task<Tuple<Contract, bool>> InitializeCreating(string contractOwnerId, NewCustomerDTO newCustomer, string improvmentType = null)
        {
            var contract = _contractRepository.CreateContract(contractOwnerId);

            if (contract != null)
            {
                _unitOfWork.Save();

                var customer = Mapper.Map<Customer>(newCustomer.PrimaryCustomer);

                var contractData = new ContractData
                {
                    PrimaryCustomer = customer,
                    HomeOwners = new List<Customer> {customer},
                    DealerId = contractOwnerId,
                    Id = contract.Id,
                    Equipment = new EquipmentInfo
                    {
                        EstimatedInstallationDate = newCustomer.EstimatedMoveInDate
                    }
                };

                if (!string.IsNullOrEmpty(improvmentType))
                {
                    contractData.Equipment.NewEquipment = new List<NewEquipment> { new NewEquipment { Type = improvmentType } };
                }

                return await UpdateNewContractForCustomer(contractOwnerId, newCustomer, contractData);
            }

           _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}] with improvment type [{improvmentType}]");

            return new Tuple<Contract, bool>(null, false);
        }

        private async Task<Tuple<Contract, bool>> UpdateNewContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer, ContractData contractData)
        {
            var updatedContract = _contractRepository.UpdateContractData(contractData, contractOwnerId);

            if (updatedContract == null)
            {
                return new Tuple<Contract, bool>(null, false);
            }

            _unitOfWork.Save();

            updatedContract.IsCreatedByBroker = true;
            _unitOfWork.Save();

            if (updatedContract.PrimaryCustomer != null)
            {
                await _aspireService.UpdateContractCustomer(updatedContract.Id, contractOwnerId);
            }

            if (updatedContract.Details != null && newCustomer.CustomerComment != null)
            {
                if (string.IsNullOrEmpty(updatedContract.Details.Notes))
                {
                    updatedContract.Details.Notes = newCustomer.CustomerComment;
                }
                else
                {
                    updatedContract.Details.Notes += newCustomer.CustomerComment;
                }
            }

            return new Tuple<Contract, bool>(updatedContract, true);
        }
    }
}
