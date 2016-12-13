using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Infrastructure
{
    public class ContractManager : IContractManager
    {
        private readonly IScanProcessingServiceAgent _scanProcessingServiceAgent;
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public ContractManager(IScanProcessingServiceAgent scanProcessingServiceAgent, IContractServiceAgent contractServiceAgent,
            IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        public async Task<DealItemOverviewViewModel> GetWorkItemsAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId)
        {
            var basicInfo = new BasicInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return basicInfo;
            }
            basicInfo.ContractId = contractId;
            await MapBasicInfo(basicInfo, contractResult.Item1);
            return basicInfo;
        }

        public async Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId)
        {
            var contactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return contactAndPaymentInfo;
            }
            contactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(contactAndPaymentInfo, contractResult.Item1);
            return contactAndPaymentInfo;
        }

        public async Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId)
        {
            var equipmentInfo = new EquipmentInformationViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return equipmentInfo;
            }
            equipmentInfo.ContractId = contractId;
            if (contractResult.Item1.Equipment != null)
            {
                equipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            }
            var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(contractResult.Item1.PrimaryCustomer.Locations.First(
                        l => l.AddressType == AddressType.MainAddress).State.ToProvinceCode())).Item1;
            if (rate != null) { equipmentInfo.ProvinceTaxRate = rate.Rate; }
            equipmentInfo.CreditAmount = contractResult.Item1.Details?.CreditAmount;
            equipmentInfo.IsAllInfoCompleted = contractResult.Item1.PaymentInfo != null && contractResult.Item1.PrimaryCustomer?.Phones != null && contractResult.Item1.PrimaryCustomer.Phones.Any();
            if (!equipmentInfo.RequestedTerm.HasValue)
            {
                equipmentInfo.RequestedTerm = 120;
            }
            return equipmentInfo;
        }

        public async Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId, ContractDTO contract = null)
        {
            var summaryAndConfirmation = new SummaryAndConfirmationViewModel();
            var contractResult = contract ?? (await _contractServiceAgent.GetContract(contractId))?.Item1;
            if (contractResult == null)
            {
                return summaryAndConfirmation;
            }
            await MapSummary(summaryAndConfirmation, contractResult, contractId);
            return summaryAndConfirmation;
        }

        public async Task<IList<ContractViewModel>> GetContractsAsync(IEnumerable<int> ids)
        {
            var contractsResult = await _contractServiceAgent.GetContracts(ids);
            if (contractsResult.Item1 == null)
            {
                return new List<ContractViewModel>();
            }
            var contracts = new List<ContractViewModel>();
            foreach (var contract in contractsResult.Item1)
            {
                var contractViewModel = new ContractViewModel();
                await MapContract(contractViewModel, contract, contract.Id);
                contracts.Add(contractViewModel);
            }
            return contracts;
        }

        public async Task<ContractViewModel> GetContractAsync(int contractId)
        {
            var contractsResult = await _contractServiceAgent.GetContract(contractId);
            if (contractsResult.Item1 == null) { return null; }
            var contractViewModel = new ContractViewModel();
            await MapContract(contractViewModel, contractsResult.Item1, contractsResult.Item1.Id);
            return contractViewModel;
        }

        public async Task<ContractEditViewModel> GetContractEditAsync(int contractId)
        {
            var contractsResult = await _contractServiceAgent.GetContract(contractId);
            if (contractsResult == null) { return null; }
            var summaryViewModel = await GetSummaryAndConfirmationAsync(contractId, contractsResult.Item1);

            var contractEditViewModel = new ContractEditViewModel()
            {
                AdditionalInfo = summaryViewModel.AdditionalInfo,
                ContactAndPaymentInfo = summaryViewModel.ContactAndPaymentInfo,
                BasicInfo = summaryViewModel.BasicInfo,
                EquipmentInfo = summaryViewModel.EquipmentInfo,
                ProvinceTaxRate = summaryViewModel.ProvinceTaxRate,
                LoanCalculatorOutput = summaryViewModel.LoanCalculatorOutput
            };
            var comments = AutoMapper.Mapper.Map<List<CommentViewModel>>(contractsResult.Item1.Comments);
            comments?.Reverse();
            contractEditViewModel.Comments = comments;

            contractEditViewModel.UploadDocumentsInfo = new UploadDocumentsViewModel();
            contractEditViewModel.UploadDocumentsInfo.ExistingDocuments = Mapper.Map<List<ExistingDocument>>(contractsResult.Item1.Documents);
            contractEditViewModel.UploadDocumentsInfo.DocumentsForUpload = new List<DocumentForUpload>();
            var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();
            if (docTypes?.Item1 != null)
            {
                contractEditViewModel.UploadDocumentsInfo.DocumentTypes = docTypes.Item1.Select(d => new SelectListItem()
                {
                    Value = d.Id.ToString(),
                    Text = d.Description                                        
                }).ToList();
            }

            return contractEditViewModel;
        }

        public async Task MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract)
        {
            basicInfo.HomeOwner = AutoMapper.Mapper.Map<ApplicantPersonalInfo>(contract.PrimaryCustomer);
            basicInfo.AdditionalApplicants = AutoMapper.Mapper.Map<List<ApplicantPersonalInfo>>(contract.SecondaryCustomers);

            basicInfo.SubmittingDealerId = contract.ExternalSubDealerId ?? contract.DealerId;
            basicInfo.SubDealers = new List<SubDealer>();
            var dealerInfo = await _dictionaryServiceAgent.GetDealerInfo();
            if ((dealerInfo?.SubDealers?.Any() ?? false) || (dealerInfo?.UdfSubDealers?.Any() ?? false))
            {
                var mainDealer = new SubDealer()
                {
                    Id = dealerInfo.Id,
                    DisplayName = "On my behalf"
                };
                basicInfo.SubDealers.Add(mainDealer);
                basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.SubDealers));
                if (dealerInfo?.UdfSubDealers?.Any() ?? false)
                {
                    basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.UdfSubDealers));
                }
            }
        }

        public void MapContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo, ContractDTO contract)
        {
            contactAndPaymentInfo.PaymentInfo = AutoMapper.Mapper.Map<PaymentInfoViewModel>(
                    contract.PaymentInfo);
            contactAndPaymentInfo.HomeOwnerContactInfo = AutoMapper.Mapper.Map<ContactInfoViewModel>(
                    contract.PrimaryCustomer);
            contactAndPaymentInfo.HouseSize = contract.Details.HouseSize;
            contactAndPaymentInfo.CoBorrowersContactInfo = AutoMapper.Mapper.Map<List<ContactInfoViewModel>>(contract.SecondaryCustomers);
        }

        public async Task<IList<Alert>> UpdateContractAsync(BasicInfoViewModel basicInfo)
        {
            var contractData = new ContractDataDTO();
            contractData.Id = basicInfo.ContractId ?? 0;
            if (!string.IsNullOrEmpty(basicInfo.SubmittingDealerId))
            {
                //check dealer for update
                var dealerInfo = await _dictionaryServiceAgent.GetDealerInfo();
                var usd = dealerInfo?.UdfSubDealers?.FirstOrDefault(usdl => usdl.SubmissionValue == basicInfo.SubmittingDealerId);
                if (usd != null)
                {
                    contractData.ExternalSubDealerId = usd.SubmissionValue;
                    contractData.ExternalSubDealerName = usd.SubDealerName;
                }
                else
                {
                    contractData.DealerId = basicInfo.SubmittingDealerId;
                    contractData.ExternalSubDealerId = string.Empty;
                    contractData.ExternalSubDealerName = string.Empty;
                }                
            }            
            contractData.PrimaryCustomer = AutoMapper.Mapper.Map<CustomerDTO>(basicInfo.HomeOwner);
            contractData.PrimaryCustomer.Locations = new List<LocationDTO>();
            var mainAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.AddressInformation);
            mainAddress.AddressType = AddressType.MainAddress;
            contractData.PrimaryCustomer.Locations.Add(mainAddress);
            if (basicInfo.HomeOwner.MailingAddressInformation != null)
            {
                var mailAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.MailingAddressInformation);
                mailAddress.AddressType = AddressType.MailAddress;
                contractData.PrimaryCustomer.Locations.Add(mailAddress);
            }

            contractData.SecondaryCustomers = new List<CustomerDTO>();
            basicInfo.AdditionalApplicants?.ForEach(a =>
            {
                var customer = AutoMapper.Mapper.Map<CustomerDTO>(a);
                customer.Locations = new List<LocationDTO>();
                if (a.MailingAddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(a.MailingAddressInformation);
                    mailAddress.AddressType = AddressType.MailAddress;
                    customer.Locations.Add(mailAddress);
                }
                contractData.SecondaryCustomers.Add(customer);
            });
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                Equipment = AutoMapper.Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };
            if (equipmnetInfo.ExistingEquipment == null)
            {
                contractData.Equipment.ExistingEquipment = new List<ExistingEquipmentDTO>();
            }
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            var alerts = new List<Alert>();

            var contractData = new ContractDataDTO { Id = contactAndPaymentInfo.ContractId ?? 0 };

            List<CustomerDataDTO> customers = new List<CustomerDataDTO>();
            if (contactAndPaymentInfo.HomeOwnerContactInfo != null)
            {
                customers.Add(Mapper.Map<CustomerDataDTO>(contactAndPaymentInfo.HomeOwnerContactInfo));
            }
            if (contactAndPaymentInfo.CoBorrowersContactInfo?.Any() ?? false)
            {
                contactAndPaymentInfo.CoBorrowersContactInfo.ForEach(cnt =>
                    customers.Add(Mapper.Map<CustomerDataDTO>(cnt)));
            }

            if (customers.Any())
            {
                customers.ForEach(c => c.ContractId = contactAndPaymentInfo.ContractId);
                alerts.AddRange(await _contractServiceAgent.UpdateCustomerData(customers.ToArray()));
            }

            if (contactAndPaymentInfo.HouseSize.HasValue)
            {
                contractData.Details = new ContractDetailsDTO()
                {
                    HouseSize = contactAndPaymentInfo.HouseSize
                };
            }

            if (contactAndPaymentInfo.PaymentInfo != null)
            {
                var paymentInfo = AutoMapper.Mapper.Map<PaymentInfoDTO>(contactAndPaymentInfo.PaymentInfo);
                contractData.PaymentInfo = paymentInfo;                
            }
            alerts.AddRange(await _contractServiceAgent.UpdateContractData(contractData));
            return alerts;
        }

        public async Task<IList<Alert>> UpdateApplicants(BasicInfoViewModel basicInfo)
        {
            var customers = new List<CustomerDataDTO>();
            if (basicInfo.HomeOwner != null)
            {
                var customerDTO = Mapper.Map<CustomerDataDTO>(basicInfo.HomeOwner);
                customerDTO.Locations = new List<LocationDTO>();
                var mainAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.AddressInformation);
                mainAddress.AddressType = AddressType.MainAddress;
                customerDTO.Locations.Add(mainAddress);
                if (basicInfo.HomeOwner.MailingAddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.MailingAddressInformation);
                    mailAddress.AddressType = AddressType.MailAddress;
                    customerDTO.Locations.Add(mailAddress);
                }
                customers.Add(customerDTO);
            }
            basicInfo.AdditionalApplicants?.ForEach(applicant =>
            {
                var customerDTO = Mapper.Map<CustomerDataDTO>(applicant);
                customerDTO.Locations = new List<LocationDTO>();
                var mailAddress = Mapper.Map<LocationDTO>(applicant.MailingAddressInformation);
                mailAddress.AddressType = AddressType.MailAddress;
                customerDTO.Locations.Add(mailAddress);
                customers.Add(customerDTO);
            });
            customers.ForEach(c => c.ContractId = basicInfo.ContractId);
            return await _contractServiceAgent.UpdateCustomerData(customers.ToArray());
        }

        public async Task<Tuple<int?, IList<Alert>>> CreateNewCustomerContract(int contractId)
        {
            int? newContractId = null;
            var alerts = new List<Alert>();

            var contractRes = await _contractServiceAgent.GetContract(contractId);
            if (contractRes.Item2.Any())
            {
                alerts.AddRange(contractRes.Item2);
            }
            if (contractRes.Item1 != null && contractRes.Item2.All(a => a.Type != AlertType.Error))
            {
                var customer = contractRes.Item1.PrimaryCustomer;

                var newContractRes = await _contractServiceAgent.CreateContract();
                if (newContractRes.Item2.Any())
                {
                    alerts.AddRange(newContractRes.Item2);
                }
                if (newContractRes.Item1 != null && newContractRes.Item2.All(a => a.Type != AlertType.Error))
                {
                    newContractId = newContractRes.Item1.Id;
                    var contractData = new ContractDataDTO()
                    {
                        Id = newContractRes.Item1.Id,
                        PrimaryCustomer = customer,
                        PaymentInfo = new PaymentInfoDTO()
                        {
                            AccountNumber = contractRes.Item1.PaymentInfo.AccountNumber,
                            BlankNumber = contractRes.Item1.PaymentInfo.BlankNumber,
                            EnbridgeGasDistributionAccount = contractRes.Item1.PaymentInfo.EnbridgeGasDistributionAccount,
                            MeterNumber = contractRes.Item1.PaymentInfo.MeterNumber,
                            PaymentType = contractRes.Item1.PaymentInfo.PaymentType,
                            PrefferedWithdrawalDate = contractRes.Item1.PaymentInfo.PrefferedWithdrawalDate,
                            TransitNumber = contractRes.Item1.PaymentInfo.TransitNumber
                        }
                    };
                    var updateRes = await _contractServiceAgent.UpdateContractData(contractData);
                    if (updateRes.Any())
                    {
                        alerts.AddRange(updateRes);
                    }
                }
            }

            return new Tuple<int?, IList<Alert>>(newContractId, alerts);
        }

        private async Task MapSummary(SummaryAndConfirmationViewModel summary, ContractDTO contract, int contractId)
        {
            summary.BasicInfo = new BasicInfoViewModel();
            summary.BasicInfo.ContractId = contractId;
            await MapBasicInfo(summary.BasicInfo, contract);
            summary.EquipmentInfo = new EquipmentInformationViewModel();
            summary.EquipmentInfo.ContractId = contractId;
            summary.EquipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contract.Equipment);
            if (summary.EquipmentInfo != null)
            {
                summary.EquipmentInfo.CreditAmount = contract.Details?.CreditAmount;
            }
            summary.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summary.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summary.ContactAndPaymentInfo, contract);
            if (summary.BasicInfo.HomeOwner?.AddressInformation != null) { 
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(summary.BasicInfo.HomeOwner.AddressInformation.Province.ToProvinceCode())).Item1;
                if (rate != null) { summary.ProvinceTaxRate = rate.Rate; }
            }
                        
            summary.AdditionalInfo = new AdditionalInfoViewModel();
            summary.AdditionalInfo.ContractState = contract.ContractState;
            summary.AdditionalInfo.Status = contract.Details?.Status ?? contract.ContractState.GetEnumDescription();
            summary.AdditionalInfo.LastUpdateTime = contract.LastUpdateTime;
            summary.AdditionalInfo.TransactionId = contract.Details?.TransactionId;
            if (contract.Equipment != null && contract.Equipment.AgreementType == AgreementType.LoanApplication)
            {
                var loanCalculatorInput = new LoanCalculator.Input
                {
                    TaxRate = summary.ProvinceTaxRate,
                    LoanTerm = contract.Equipment.LoanTerm ?? 0,
                    AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                    EquipmentCashPrice = (double?)contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                    AdminFee = contract.Equipment.AdminFee ?? 0,
                    DownPayment = contract.Equipment.DownPayment ?? 0,
                    CustomerRate = contract.Equipment.CustomerRate ?? 0
                };
                summary.LoanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
            }
        }

        private async Task MapContract(ContractViewModel contractViewModel, ContractDTO contract, int contractId)
        {
            var summaryViewModel = await GetSummaryAndConfirmationAsync(contractId, contract);

            contractViewModel.AdditionalInfo = summaryViewModel.AdditionalInfo;
            contractViewModel.ContactAndPaymentInfo = summaryViewModel.ContactAndPaymentInfo;
            contractViewModel.BasicInfo = summaryViewModel.BasicInfo;
            contractViewModel.EquipmentInfo = summaryViewModel.EquipmentInfo;
            contractViewModel.ProvinceTaxRate = summaryViewModel.ProvinceTaxRate;
            contractViewModel.LoanCalculatorOutput = summaryViewModel.LoanCalculatorOutput;
            
            contractViewModel.UploadDocumentsInfo = new UploadDocumentsViewModel();
            contractViewModel.UploadDocumentsInfo.ExistingDocuments = Mapper.Map<List<ExistingDocument>>(contract.Documents);
            contractViewModel.UploadDocumentsInfo.DocumentsForUpload = new List<DocumentForUpload>();
            var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();
            if (docTypes?.Item1 != null)
            {
                contractViewModel.UploadDocumentsInfo.DocumentTypes = docTypes.Item1.Select(d => new SelectListItem()
                {
                    Value = d.Id.ToString(),
                    Text = d.Description
                }).ToList();
            }
        }
    }
}
