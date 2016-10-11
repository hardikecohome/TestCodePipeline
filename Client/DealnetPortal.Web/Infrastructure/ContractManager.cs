using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public ContractManager(IScanProcessingServiceAgent scanProcessingServiceAgent, IContractServiceAgent contractServiceAgent)
        {
            _scanProcessingServiceAgent = scanProcessingServiceAgent;
            _contractServiceAgent = contractServiceAgent;
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
            MapBasicInfo(basicInfo, contractResult.Item1);
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
            equipmentInfo.IsAllInfoCompleted = contractResult.Item1.PaymentInfo != null && contractResult.Item1.PrimaryCustomer?.Phones != null && contractResult.Item1.PrimaryCustomer.Phones.Any();
            return equipmentInfo;
        }

        public async Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId)
        {
            var summaryAndConfirmation = new SummaryAndConfirmationViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return summaryAndConfirmation;
            }
            summaryAndConfirmation.BasicInfo = new BasicInfoViewModel();
            summaryAndConfirmation.BasicInfo.ContractId = contractId;
            MapBasicInfo(summaryAndConfirmation.BasicInfo, contractResult.Item1);
            summaryAndConfirmation.EquipmentInfo = new EquipmentInformationViewModel();
            summaryAndConfirmation.EquipmentInfo.ContractId = contractId;
            summaryAndConfirmation.EquipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
            summaryAndConfirmation.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summaryAndConfirmation.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summaryAndConfirmation.ContactAndPaymentInfo, contractResult.Item1);
            summaryAndConfirmation.SendEmails = new SendEmailsViewModel();
            var rate = (await _contractServiceAgent.GetProvinceTaxRate(summaryAndConfirmation.BasicInfo.AddressInformation.Province.ToProvinceCode())).Item1;
            if (rate != null) { summaryAndConfirmation.ProvinceTaxRate = rate.Rate; }
            summaryAndConfirmation.SendEmails.ContractId = contractId;
            summaryAndConfirmation.SendEmails.HomeOwnerFullName = summaryAndConfirmation.BasicInfo.HomeOwner.FirstName + " " + summaryAndConfirmation.BasicInfo.HomeOwner.LastName;
            summaryAndConfirmation.AdditionalInfo = new AdditionalInfoViewModel();
            summaryAndConfirmation.AdditionalInfo.ContractState = contractResult.Item1.ContractState;
            summaryAndConfirmation.AdditionalInfo.LastUpdateTime = contractResult.Item1.LastUpdateTime;
            return summaryAndConfirmation;
        }

        public void MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract)
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
                alerts.AddRange(await _contractServiceAgent.UpdateCustomerData(customers.ToArray()));
            }

            if (contactAndPaymentInfo.PaymentInfo != null)
            {
                var paymentInfo = AutoMapper.Mapper.Map<PaymentInfoDTO>(contactAndPaymentInfo.PaymentInfo);
                contractData.PaymentInfo = paymentInfo;
                alerts.AddRange(await _contractServiceAgent.UpdateContractData(contractData));
            }
            return alerts;
        }
    }
}
