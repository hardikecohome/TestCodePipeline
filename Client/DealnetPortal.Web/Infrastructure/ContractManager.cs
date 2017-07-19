using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;
using ContractState = DealnetPortal.Web.Models.Enumeration.ContractState;

namespace DealnetPortal.Web.Infrastructure
{
    public class ContractManager : IContractManager
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public ContractManager(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
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

        public async Task<ContactAndPaymentInfoViewModelNew> GetAdditionalContactInfoAsyncNew(int contractId)
        {
            var contactAndPaymentInfo = new ContactAndPaymentInfoViewModelNew();
            var contractResult = await _contractServiceAgent.GetContract(contractId);

            if (contractResult.Item1 == null)
            {
                return contactAndPaymentInfo;
            }

            contactAndPaymentInfo.Notes = contractResult.Item1.Details.Notes;
            contactAndPaymentInfo.HouseSize = contractResult.Item1.Details.HouseSize;
            contactAndPaymentInfo.EstimatedInstallationDate = contractResult.Item1.Equipment.EstimatedInstallationDate;
            contactAndPaymentInfo.SalesRep = contractResult.Item1.Equipment.SalesRep;
            contactAndPaymentInfo.IsApplicantsInfoEditAvailable = contractResult.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;
            contactAndPaymentInfo.ContractId = contractId;
            contactAndPaymentInfo.AgreementType = contractResult.Item1.Equipment.AgreementType.ConvertTo<Models.Enumeration.AgreementType>();
            contactAndPaymentInfo.ExistingEquipment = Mapper.Map<List<ExistingEquipmentInformation>>(contractResult.Item1.Equipment.ExistingEquipment);

            return contactAndPaymentInfo;
        }

        public async Task<ContactAndPaymentInfoViewModel> GetContactAndPaymentInfoAsync(int contractId)
        {
            var contactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return contactAndPaymentInfo;
            }
            contactAndPaymentInfo.ContractId = contractResult.Item1.Id;
            contactAndPaymentInfo.IsApplicantsInfoEditAvailable = contractResult.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;
            MapContactAndPaymentInfo(contactAndPaymentInfo, contractResult.Item1);

            return contactAndPaymentInfo;
        }

        public async Task<EquipmentInformationViewModelNew> GetEquipmentInfoAsyncNew(int contractId)
        {
            Tuple<ContractDTO, IList<Alert>> result = await _contractServiceAgent.GetContract(contractId);
            
            if (result.Item1 == null)
            {
                return new EquipmentInformationViewModelNew();
            }
            
            var equipmentInfo = new EquipmentInformationViewModelNew()
            {
                ContractId = contractId,
            };

            if (result.Item1.Equipment != null)
            {
                equipmentInfo = Mapper.Map<EquipmentInformationViewModelNew>(result.Item1.Equipment);

                if (!equipmentInfo.NewEquipment.Any())
                {
                    equipmentInfo.NewEquipment = null;
                }

                if (result.Item1.Equipment.ValueOfDeal == null)
                {
                    equipmentInfo.IsNewContract = true;
                }
            }
            else
            {
                equipmentInfo.IsNewContract = true;
            }

            var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(result.Item1.PrimaryCustomer.Locations.First(
                        l => l.AddressType == AddressType.MainAddress).State.ToProvinceCode())).Item1;

            if (rate != null) { equipmentInfo.ProvinceTaxRate = rate; }

            equipmentInfo.CreditAmount = result.Item1.Details?.CreditAmount;
            equipmentInfo.IsAllInfoCompleted = result.Item1.PaymentInfo != null && result.Item1.PrimaryCustomer?.Phones != null && result.Item1.PrimaryCustomer.Phones.Any();
            equipmentInfo.IsApplicantsInfoEditAvailable = result.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;

            if (!equipmentInfo.RequestedTerm.HasValue)
            {
                equipmentInfo.RequestedTerm = 120;
            }

            equipmentInfo.CreditAmount = result.Item1.Details?.CreditAmount;
            var dealerTier = await _contractServiceAgent.GetDealerTier();
            equipmentInfo.DealerTier = dealerTier ?? new TierDTO() {RateCards = new List<RateCardDTO>()};

            AddAditionalContractInfo(result.Item1, equipmentInfo);

            if (result.Item1.Comments.Any(x => x.IsCustomerComment == true))
            {
                var comments = result.Item1.Comments
                    .Where(x => x.IsCustomerComment == true)
                    .Select(q => q.Text)
                    .ToList();

                equipmentInfo.CustomerComments = comments;
            }

            return equipmentInfo;
        }

        public async Task<EquipmentInformationViewModel> GetEquipmentInfoAsync(int contractId)
        {            
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if (contractResult.Item1 == null)
            {
                return new EquipmentInformationViewModel();
            }
            var equipmentInfo = new EquipmentInformationViewModel()
            {
                ContractId = contractId,
            };
            if (contractResult.Item1.Equipment != null)
            {
                equipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
                if (!equipmentInfo.NewEquipment.Any())
                {
                    equipmentInfo.NewEquipment = null;
                }
                if (!equipmentInfo.ExistingEquipment.Any())
                {
                    equipmentInfo.ExistingEquipment = null;
                }
            }
            equipmentInfo.Notes = contractResult.Item1.Details?.Notes;

            var mainAddressProvince = contractResult.Item1.PrimaryCustomer.Locations
                .FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State.ToProvinceCode();

            if (mainAddressProvince != null)
            {
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(mainAddressProvince)).Item1;
                if (rate != null) { equipmentInfo.ProvinceTaxRate = rate; }
            }

            equipmentInfo.CreditAmount = contractResult.Item1.Details?.CreditAmount;
            equipmentInfo.IsAllInfoCompleted = contractResult.Item1.PaymentInfo != null && contractResult.Item1.PrimaryCustomer?.Phones != null && contractResult.Item1.PrimaryCustomer.Phones.Any();
            equipmentInfo.IsApplicantsInfoEditAvailable = contractResult.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;
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
                LoanCalculatorOutput = summaryViewModel.LoanCalculatorOutput,
                Notes = summaryViewModel.Notes
            };

            if (contractsResult.Item1.Comments != null)
            {
                var notByCustomerComments = contractsResult.Item1.Comments
                    .Where(x => x.IsCustomerComment != true)
                    .ToList();

                var byCustomerComments = contractsResult.Item1.Comments
                    .Where(x => x.IsCustomerComment == true)
                    .Select(q => q.Text)
                    .ToList();

                var comments = Mapper.Map<List<CommentViewModel>>(notByCustomerComments);
                comments?.Reverse();

                contractEditViewModel.Comments = comments;
                contractEditViewModel.CustomerComments = byCustomerComments;
            }

            contractEditViewModel.UploadDocumentsInfo = new UploadDocumentsViewModel();
            contractEditViewModel.UploadDocumentsInfo.ExistingDocuments = Mapper.Map<List<ExistingDocument>>(contractsResult.Item1.Documents);            
            contractEditViewModel.UploadDocumentsInfo.DocumentsForUpload = new List<DocumentForUpload>();
            contractEditViewModel.UploadDocumentsInfo.MandatoryDocumentTypes = new List<int>() { (int)DocumentTemplateType.SignedContract, (int)DocumentTemplateType.SignedInstallationCertificate, 3, 4 };
            var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();
            if (docTypes?.Item1 != null)
            {
                contractEditViewModel.UploadDocumentsInfo.DocumentTypes = docTypes.Item1.Select(d => new SelectListItem()
                {
                    Value = d.Id.ToString(),
                    Text = d.Description                                        
                }).ToList();
            }

            contractEditViewModel.InstallCertificateInformation = new CertificateInformationViewModel()
            {
                ContractId = contractId,
                InstallationDate = contractsResult.Item1.Equipment?.InstallationDate,
                InstallerFirstName = contractsResult.Item1.Equipment?.InstallerFirstName,
                InstallerLastName = contractsResult.Item1.Equipment?.InstallerLastName,
                Equipments = new List<CertificateEquipmentInfoViewModel>()
            };
            contractsResult.Item1.Equipment?.NewEquipment?.ForEach(eq =>
                contractEditViewModel.InstallCertificateInformation.Equipments.Add(
                    new CertificateEquipmentInfoViewModel()
                    {
                        Id = eq.Id,
                        Model = eq.InstalledModel,
                        SerialNumber = eq.InstalledSerialNumber,
                    }));

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
                    DisplayName = Resources.Resources.OnMyBehalf
                };
                basicInfo.SubDealers.Add(mainDealer);
                if (dealerInfo.SubDealers?.Any() ?? false)
                {
                    basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.SubDealers));
                }
                if (dealerInfo.UdfSubDealers?.Any() ?? false)
                {
                    basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.UdfSubDealers));
                }
            }
            basicInfo.ContractState = contract.ContractState;
            basicInfo.ContractWasDeclined = contract.WasDeclined ?? false;
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
            if (basicInfo.HomeOwner.PreviousAddressInformation != null)
            {
                var previousAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.PreviousAddressInformation);
                previousAddress.AddressType = AddressType.PreviousAddress;
                contractData.PrimaryCustomer.Locations.Add(previousAddress);
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
                if (a.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(a.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customer.Locations.Add(previousAddress);
                }
                contractData.SecondaryCustomers.Add(customer);                
            });
            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsyncNew(EquipmentInformationViewModelNew equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                Equipment = Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };

            var existingEquipment = Mapper.Map<List<ExistingEquipmentDTO>>(equipmnetInfo.ExistingEquipment);
            contractData.Equipment.ExistingEquipment = existingEquipment ?? new List<ExistingEquipmentDTO>();
            contractData.Equipment.SalesRep = equipmnetInfo.SalesRep;
            contractData.Equipment.EstimatedInstallationDate = equipmnetInfo.EstimatedInstallationDate;

            contractData.Details = new ContractDetailsDTO
            {
                Notes = equipmnetInfo.Notes
            };

            if (equipmnetInfo.HouseSize.HasValue)
            {
                contractData.Details.HouseSize = equipmnetInfo.HouseSize;
            }

            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                Equipment = Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };

            if (equipmnetInfo.FullUpdate && equipmnetInfo.ExistingEquipment == null)
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

            //if (contactAndPaymentInfo.HouseSize.HasValue)
            //{
            //    contractData.Details = new ContractDetailsDTO()
            //    {
            //        HouseSize = contactAndPaymentInfo.HouseSize
            //    };
            //}

            if (contactAndPaymentInfo.PaymentInfo != null)
            {
                var paymentInfo = AutoMapper.Mapper.Map<PaymentInfoDTO>(contactAndPaymentInfo.PaymentInfo);
                contractData.PaymentInfo = paymentInfo;                
            }
            alerts.AddRange(await _contractServiceAgent.UpdateContractData(contractData));
            return alerts;
        }

        public async Task<IList<Alert>> UpdateApplicants(ApplicantsViewModel basicInfo)
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
                if (basicInfo.HomeOwner.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customerDTO.Locations.Add(previousAddress);
                }
                customers.Add(customerDTO);
            }
            basicInfo.AdditionalApplicants?.ForEach(applicant =>
            {
                var customerDTO = Mapper.Map<CustomerDataDTO>(applicant);
                customerDTO.Locations = new List<LocationDTO>();
                if (applicant.MailingAddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(applicant.MailingAddressInformation);
                    mailAddress.AddressType = AddressType.MailAddress;
                    customerDTO.Locations.Add(mailAddress);
                }
                if (applicant.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(applicant.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customerDTO.Locations.Add(previousAddress);
                }
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
                customer.Id = 0;

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
                    };

                    if (contractRes.Item1.PaymentInfo != null)
                    {
                        contractData.PaymentInfo = new PaymentInfoDTO()
                        {
                            AccountNumber = contractRes.Item1.PaymentInfo.AccountNumber,
                            BlankNumber = contractRes.Item1.PaymentInfo.BlankNumber,
                            EnbridgeGasDistributionAccount =
                                contractRes.Item1.PaymentInfo.EnbridgeGasDistributionAccount,
                            MeterNumber = contractRes.Item1.PaymentInfo.MeterNumber,
                            PaymentType = contractRes.Item1.PaymentInfo.PaymentType,
                            PrefferedWithdrawalDate = contractRes.Item1.PaymentInfo.PrefferedWithdrawalDate,
                            TransitNumber = contractRes.Item1.PaymentInfo.TransitNumber
                        };
                    }

                    var updateRes = await _contractServiceAgent.UpdateContractData(contractData);
                    if (updateRes.Any())
                    {
                        alerts.AddRange(updateRes);
                    }

                    var updatedContractRes = await _contractServiceAgent.GetContract(newContractId.Value);
                    if (updatedContractRes.Item2.Any())
                    {
                        alerts.AddRange(updatedContractRes.Item2);
                    }
                    if (updatedContractRes.Item1?.PrimaryCustomer != null && updatedContractRes.Item2.All(a => a.Type != AlertType.Error))
                    {
                        var updatedCustomer = new CustomerDataDTO()
                        {
                            Id = updatedContractRes.Item1.PrimaryCustomer.Id,
                            ContractId = newContractId,
                            Emails = contractRes.Item1.PrimaryCustomer.Emails,
                            Phones = contractRes.Item1.PrimaryCustomer.Phones,
                        };
                        await _contractServiceAgent.UpdateCustomerData(new CustomerDataDTO[] { updatedCustomer });
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
            summary.EquipmentInfo = AutoMapper.Mapper.Map<EquipmentInformationViewModel>(contract.Equipment);                        
            if (summary.EquipmentInfo != null)
            {
                summary.EquipmentInfo.CreditAmount = contract.Details?.CreditAmount;
                summary.EquipmentInfo.IsApplicantsInfoEditAvailable = contract.ContractState < Api.Common.Enumeration.ContractState.Completed;
                summary.EquipmentInfo.Notes = contract.Details?.Notes;
            }
            summary.Notes = contract.Details?.Notes;
            summary.AdditionalInfo = new AdditionalInfoViewModel();

            if (contract?.Comments.Any(x => x.IsCustomerComment == true) == true)
            {
                var comments = contract.Comments
                    .Where(x => x.IsCustomerComment == true)
                    .Select(q => q.Text)
                    .ToList();

                summary.AdditionalInfo.CustomerComments = comments;
            }
           
            summary.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel();
            summary.ContactAndPaymentInfo.ContractId = contractId;
            MapContactAndPaymentInfo(summary.ContactAndPaymentInfo, contract);
            if (summary.BasicInfo.HomeOwner?.AddressInformation != null) { 
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(summary.BasicInfo.HomeOwner.AddressInformation.Province.ToProvinceCode())).Item1;
                if (rate != null) { summary.ProvinceTaxRate = rate; }
            }
                        
            summary.AdditionalInfo.ContractState = contract.ContractState.ConvertTo<ContractState>();
            summary.AdditionalInfo.Status = contract.Details?.Status ?? contract.ContractState.GetEnumDescription();
            summary.AdditionalInfo.LastUpdateTime = contract.LastUpdateTime;
            summary.AdditionalInfo.TransactionId = contract.Details?.TransactionId;
            summary.AdditionalInfo.IsCreatedByCustomer = contract.IsCreatedByCustomer ?? false;
            if (contract.Equipment != null && contract.Equipment.AgreementType == AgreementType.LoanApplication)
            {
                var loanCalculatorInput = new LoanCalculator.Input
                {
                    TaxRate = summary.ProvinceTaxRate.Rate,
                    LoanTerm = contract.Equipment.LoanTerm ?? 0,
                    AmortizationTerm = contract.Equipment.AmortizationTerm ?? 0,
                    EquipmentCashPrice = (double?)contract.Equipment?.NewEquipment.Sum(x => x.Cost) ?? 0,
                    AdminFee = contract.Equipment.AdminFee ?? 0,
                    DownPayment = contract.Equipment.DownPayment ?? 0,
                    CustomerRate = contract.Equipment.CustomerRate ?? 0
                };
                summary.LoanCalculatorOutput = loanCalculatorInput.AmortizationTerm > 0 ? LoanCalculator.Calculate(loanCalculatorInput) : new LoanCalculator.Output();
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
            contractViewModel.UploadDocumentsInfo.MandatoryDocumentTypes = new List<int>() {(int)DocumentTemplateType.SignedContract, (int)DocumentTemplateType.SignedInstallationCertificate, 3, 4};
            var docTypes = await _dictionaryServiceAgent.GetDocumentTypes();
            if (docTypes?.Item1 != null)
            {
                contractViewModel.UploadDocumentsInfo.DocumentTypes = docTypes.Item1.Select(d => new SelectListItem()
                {
                    Value = d.Id.ToString(),
                    Text = d.Description
                }).ToList();
            }

            if (contract.Comments != null && contract.Comments.Any(x => x.IsCustomerComment == true))
            {
                var comments = contract.Comments
                    .Where(x => x.IsCustomerComment == true)
                    .Select(q => q.Text)
                    .ToList();

                contractViewModel.AdditionalInfo.CustomerComments = comments;
            }
        }

        private void AddAditionalContractInfo(ContractDTO contract, EquipmentInformationViewModelNew equipmentInfo)
        {
            equipmentInfo.Notes = contract.Details.Notes;
            equipmentInfo.HouseSize = contract.Details.HouseSize;
            equipmentInfo.IsApplicantsInfoEditAvailable = contract.ContractState < Api.Common.Enumeration.ContractState.Completed;

            if (contract.Equipment != null)
            {
                equipmentInfo.EstimatedInstallationDate = contract.Equipment.EstimatedInstallationDate;
                equipmentInfo.SalesRep = contract.Equipment.SalesRep;
                equipmentInfo.ExistingEquipment = Mapper.Map<List<ExistingEquipmentInformation>>(contract.Equipment.ExistingEquipment);
            }
        }
    }
}

