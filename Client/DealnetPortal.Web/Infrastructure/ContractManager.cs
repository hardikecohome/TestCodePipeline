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
            equipmentInfo.IsAllInfoCompleted = contractResult.Item1.PaymentInfo != null && contractResult.Item1.PrimaryCustomer?.Phones != null && contractResult.Item1.PrimaryCustomer.Phones.Any();
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

        public async Task<IList<SummaryAndConfirmationViewModel>> GetSummaryAndConfirmationsAsync(IEnumerable<int> ids)
        {
            var contractsResult = await _contractServiceAgent.GetContracts(ids);
            if (contractsResult.Item1 == null)
            {
                return new List<SummaryAndConfirmationViewModel>();
            }
            var summaries = new List<SummaryAndConfirmationViewModel>();
            foreach (var contract in contractsResult.Item1)
            {
                var summaryAndConfirmation = new SummaryAndConfirmationViewModel();
                await MapSummary(summaryAndConfirmation, contract, contract.Id);
                summaries.Add(summaryAndConfirmation);
            }
            return summaries;
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

            basicInfo.AddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MainAddress));
            basicInfo.MailingAddressInformation =
                AutoMapper.Mapper.Map<AddressInformation>(
                    contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                        l => l.AddressType == AddressType.MailAddress));

            basicInfo.SubmittingDealerId = contract.DealerId;
            basicInfo.SubDealers = new List<SubDealer>();
            var dealerInfo = await _dictionaryServiceAgent.GetDealerInfo();
            if (dealerInfo?.SubDealers?.Any() ?? false)
            {
                var mainDealer = new SubDealer()
                {
                    Id = dealerInfo.Id,
                    DisplayName = "On my behalf"
                };
                basicInfo.SubDealers.Add(mainDealer);
                basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.SubDealers));                
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
                contractData.DealerId = basicInfo.SubmittingDealerId;
            }            
            contractData.PrimaryCustomer = AutoMapper.Mapper.Map<CustomerDTO>(basicInfo.HomeOwner);
            contractData.SecondaryCustomers = new List<CustomerDTO>();
            if (basicInfo.AdditionalApplicants != null)
            {
                basicInfo.AdditionalApplicants.ForEach(a =>
                {
                    contractData.SecondaryCustomers.Add(AutoMapper.Mapper.Map<CustomerDTO>(a));
                });
            }
            contractData.Locations = new List<LocationDTO>();
            var address = AutoMapper.Mapper.Map<LocationDTO>(basicInfo.AddressInformation);
            address.AddressType = AddressType.MainAddress;
            contractData.Locations.Add(address);
            if (basicInfo.MailingAddressInformation != null)
            {
                address = AutoMapper.Mapper.Map<LocationDTO>(basicInfo.MailingAddressInformation);
                address.AddressType = AddressType.MailAddress;
                contractData.Locations.Add(address);
            }
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                Equipment = AutoMapper.Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };
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

        private async Task MapSummary(SummaryAndConfirmationViewModel summary, ContractDTO contract, int contractId)
        {
            summary.BasicInfo = new BasicInfoViewModel();
            summary.BasicInfo.ContractId = contractId;
            await MapBasicInfo(summary.BasicInfo, contract);
            summary.EquipmentInfo = new EquipmentInformationViewModel();
            summary.EquipmentInfo.ContractId = contractId;
            summary.EquipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contract.Equipment);
            summary.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summary.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summary.ContactAndPaymentInfo, contract);
            summary.SendEmails = new SendEmailsViewModel();
            if (summary.BasicInfo.AddressInformation != null) { 
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(summary.BasicInfo.AddressInformation.Province.ToProvinceCode())).Item1;
                if (rate != null) { summary.ProvinceTaxRate = rate.Rate; }
            }

            summary.SendEmails.ContractId = contractId;
            summary.SendEmails.HomeOwnerFullName = summary.BasicInfo.HomeOwner?.FirstName + " " + summary.BasicInfo.HomeOwner?.LastName;
            summary.SendEmails.HomeOwnerId = contract.PrimaryCustomer?.Id ?? 0;
            summary.SendEmails.HomeOwnerEmail = contract.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                contract.PrimaryCustomer?.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress;

            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                summary.SendEmails.AdditionalApplicantsEmails =
                    contract.SecondaryCustomers.Select(c =>
                        new CustomerEmail()
                        {
                            CustomerId = c.Id,
                            Email = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                        c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress
                        }).ToArray();
            }
                        
            summary.AdditionalInfo = new AdditionalInfoViewModel();
            summary.AdditionalInfo.ContractState = contract.ContractState;
            summary.AdditionalInfo.LastUpdateTime = contract.LastUpdateTime;
            if (contract.Equipment != null && contract.Equipment.AgreementType == AgreementType.LoanApplication)
            {
                var loanCalculatorInput = new LoanCalculator.Input
                {
                    TaxRate = summary.ProvinceTaxRate,
                    LoanTerm = contract.Equipment.RequestedTerm,
                    AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                    EquipmentCashPrice = (double?)contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                    AdminFee = contract.Equipment.AdminFee ?? 0,
                    DownPayment = contract.Equipment.DownPayment ?? 0,
                    CustomerRate = contract.Equipment.CustomerRate ?? 0
                };
                summary.LoanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
            }
        }
    }
}
