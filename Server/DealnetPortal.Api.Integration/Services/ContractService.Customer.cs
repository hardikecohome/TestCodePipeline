using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Integration.Services
{
    public partial class ContractService
    {
        public async Task<bool> CreateContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer)
        {
            try
            {
                var contractsResultList = new List<Tuple<int?, bool>>();

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
                    _loggingService.LogError($"Failed to create a new contract for customer by [{contractOwnerId}]");

                    return false;
                }

                //TODO: maybe it's better to try few times initiate contract check for unseccessful contracts
                var aspireFailedResults = new List<Tuple<int, bool>>();

                foreach (var contractResult in contractsResultList.Where(x => x.Item1 != null).ToList())
                {
                    var creditCheckAlerts = new List<Alert>();

                    var initAlerts = InitiateCreditCheck(contractResult.Item1.Value, contractOwnerId);

                    if (initAlerts?.Any() ?? false)
                    {
                        creditCheckAlerts.AddRange(initAlerts);
                    }

                    var checkResult = GetCreditCheckResult(contractResult.Item1.Value, contractOwnerId);
                    if (checkResult != null)
                    {
                        creditCheckAlerts.AddRange(checkResult.Item2);
                    }

                    if (creditCheckAlerts.Any(x => x.Type == AlertType.Error))
                    {
                        aspireFailedResults.Add(Tuple.Create(contractResult.Item1.Value, false));
                    }
                }                

                //if all aspire opertaion is failed
                if (aspireFailedResults.Any() )
                {
                    return false;
                }

                //select any of newly created contracts for create a new user in Customer Wallet portal
                var succededContract = _contractRepository.GetContracts(
                    contractsResultList.Where(r => r.Item2 && r.Item1.HasValue).Select(r => r.Item1.Value),
                    contractOwnerId)?
                    .FirstOrDefault(c => c.ContractState >= ContractState.CreditContirmed);
                if (succededContract != null)
                {
                    var cwCustomerResult = await _customerWalletService.CreateCustomerByContract(succededContract, contractOwnerId);
                }
                else
                {
                    _loggingService.LogWarning($"Customer contract(s) for dealer {contractOwnerId} wasn't approved on Aspire");
                }

                //TODO: return Alerts?
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to create a new contract for customer [{contractOwnerId}]", ex);
                throw;
            }
        }

        private async Task<Tuple<int?, bool>> InitializeCreating(string contractOwnerId, NewCustomerDTO newCustomer, string improvmentType = null)
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

            return new Tuple<int?, bool>(null, false);
        }

        private async Task<Tuple<int?, bool>> UpdateNewContractForCustomer(string contractOwnerId, NewCustomerDTO newCustomer, ContractData contractData)
        {
            var updatedContract = _contractRepository.UpdateContractData(contractData, contractOwnerId);

            if (updatedContract == null)
            {
                return new Tuple<int?, bool>(null, false);
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

            return new Tuple<int?, bool>(updatedContract.Id, true);
        }
    }
}
