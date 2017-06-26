using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Domain;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services
{
    public partial class ContractService
    {
		Private readonly IMandrillService _mandrillService = new MandrillService();
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

                //TODO: maybe it's better to try few times initiate contract check for unseccessful contracts
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

                    try
                    {
                        //try to submit deal in Aspire
                        await _aspireService.SendDealUDFs(contractResult.Item1.Id, contractOwnerId);
                    }
                    catch (Exception ex)
                    {
                        _loggingService.LogError($"Cannot submit deal {contractResult.Item1.Id} in Aspire", ex);
                    }
                }

                //if all aspire opertaion is failed

                var isErrors = creditCheckAlerts.Any(x => x.Type == AlertType.Error) || aspireFailedResults.Any();
                if (isErrors)
                {
                    return new Tuple<ContractDTO, IList<Alert>>(null, creditCheckAlerts);
                }

                //select any of newly created contracts for create a new user in Customer Wallet portal
                var succededContracts = contractsResultList.Where(r => r.Item1 != null && r.Item1.ContractState >= ContractState.CreditContirmed).Select(r => r.Item1).ToList();
                var succededContract = succededContracts.FirstOrDefault();
                if (succededContract != null)
                {
                    var resultAlerts = await _customerWalletService.CreateCustomerByContract(succededContract, contractOwnerId);
                    //TODO: DEAL - 1495 analyze result here and then send invite link to customer
                    if (resultAlerts.All(x => x.Type != AlertType.Error))
                    {
                        //??? - what is this?
                        if (succededContracts.Select(x => x.Equipment?.NewEquipment?.FirstOrDefault()).Any(i=>i!=null) &&
                            succededContract.PrimaryCustomer.Locations
                            .FirstOrDefault(l => l.AddressType == AddressType.InstallationAddress) !=null)
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
					//not approved log
                    //await _mandrillService.SendDeclineNotificationConfirmation(newCustomer.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                    //                                                            newCustomer.PrimaryCustomer.FirstName, newCustomer.PrimaryCustomer.LastName,
                    //                                                            newCustomer.HomeImprovementTypes.FirstOrDefault());
                    await _mailService.SendDeclinedConfirmation(newCustomer.PrimaryCustomer.Emails.FirstOrDefault().EmailAddress,
                                                                                newCustomer.PrimaryCustomer.FirstName, newCustomer.PrimaryCustomer.LastName);

                }

                //remove all newly created "internal" (unsubmitted to aspire) contracts here
                contractsResultList.Where(r => r.Item1 != null && string.IsNullOrEmpty(r.Item1.Details?.TransactionId)).ForEach(
                    cr =>
                    {                        
                        _loggingService.LogWarning($"Internal Contract {cr.Item1.Id} is removing from DB");
                        creditCheckAlerts.Add(new Alert()
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

                var contractDTO = Mapper.Map<ContractDTO>(succededContract ?? contractsResultList?.FirstOrDefault()?.Item1);
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
            var equipmentType = _contractRepository.GetEquipmentTypes();

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
                    //Equipment = new EquipmentInfo
                    //{
                    //    EstimatedInstallationDate = newCustomer.EstimatedMoveInDate
                    //}
                };

                if (!string.IsNullOrEmpty(improvmentType))
                {
                    var eq = equipmentType.SingleOrDefault(x => x.Type == improvmentType);
                    contractData.Equipment = new EquipmentInfo()
                    {
                        NewEquipment = new List<NewEquipment> { new NewEquipment { Type = improvmentType, Description = eq?.Description } }
                    };
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

        private bool IsContractUnassignable(int contractId)
        {
            return _contractRepository.IsContractUnassignable(contractId);
        }
    }
}
