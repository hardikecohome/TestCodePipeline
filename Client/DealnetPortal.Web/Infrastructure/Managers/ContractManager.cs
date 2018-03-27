using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using AutoMapper;

using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Common.Types;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using DealnetPortal.Web.Models;
using DealnetPortal.Web.Models.Enumeration;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.ServiceAgent;
using AgreementType = DealnetPortal.Api.Common.Enumeration.AgreementType;
using ContractState = DealnetPortal.Web.Models.Enumeration.ContractState;
using RateCardType = DealnetPortal.Api.Common.Enumeration.RateCardType;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public class ContractManager : IContractManager
    {
        private readonly IContractServiceAgent _contractServiceAgent;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly string _leadSource;
        private readonly string _clarityProgramTier;

        public ContractManager(IContractServiceAgent contractServiceAgent, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _contractServiceAgent = contractServiceAgent;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _leadSource = System.Configuration.ConfigurationManager.AppSettings[PortalConstants.DefaultLeadSourceKey];
            _clarityProgramTier = System.Configuration.ConfigurationManager.AppSettings[PortalConstants.ClarityTierNameKey];
        }

        public async Task<BasicInfoViewModel> GetBasicInfoAsync(int contractId)
        {
            var basicInfo = new BasicInfoViewModel();
            var contractResult = await _contractServiceAgent.GetContract(contractId);
            if(contractResult.Item1 == null)
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

            if(contractResult.Item1 == null)
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
            if(contractResult.Item1 == null)
            {
                return contactAndPaymentInfo;
            }

            contactAndPaymentInfo.ContractId = contractResult.Item1.Id;
            contactAndPaymentInfo.IsApplicantsInfoEditAvailable = contractResult.Item1.ContractState <= Api.Common.Enumeration.ContractState.Completed;
            contactAndPaymentInfo.IsFirstStepAvailable = contractResult.Item1.ContractState != Api.Common.Enumeration.ContractState.Completed;

            MapContactAndPaymentInfo(contactAndPaymentInfo, contractResult.Item1);

            return contactAndPaymentInfo;
        }

        public async Task<EquipmentInformationViewModelNew> GetEquipmentInfoAsyncNew(int contractId)
        {
            Tuple<ContractDTO, IList<Alert>> result = await _contractServiceAgent.GetContract(contractId);

            if(result.Item1 == null)
            {
                return new EquipmentInformationViewModelNew();
            }

            var equipmentInfo = new EquipmentInformationViewModelNew()
            {
                ContractId = contractId,
            };

            if(result.Item1.Equipment != null)
            {
                equipmentInfo = Mapper.Map<EquipmentInformationViewModelNew>(result.Item1.Equipment);
                equipmentInfo.SalesRepInformation = Mapper.Map<SalesRepInformation>(result.Item1);

                if(!equipmentInfo.NewEquipment.Any())
                {
                    equipmentInfo.NewEquipment = null;
                }

                if(result.Item1.Equipment.ValueOfDeal == null || result.Item1.Equipment.ValueOfDeal == 0)
                {
                    equipmentInfo.IsNewContract = true;
                    equipmentInfo.RequestedTerm = 120;
                }
                else
                {
                    var isCustomSelected = result.Item1.Equipment?.IsCustomRateCard == true || result.Item1.Equipment?.RateCardId == 0;
                    equipmentInfo.IsCustomRateCardSelected = isCustomSelected;
                }

                equipmentInfo.IsOldClarityDeal = result.Item1.Equipment.IsClarityProgram == null && equipmentInfo.IsClarityDealer;
            }
            else
            {
                equipmentInfo.IsNewContract = true;
                equipmentInfo.RequestedTerm = 120;
            }

            var mainAddressProvinceCode = result.Item1.PrimaryCustomer.Locations.First(l => l.AddressType == AddressType.MainAddress).State.ToProvinceCode();
            var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(mainAddressProvinceCode)).Item1;

            if(rate != null)
            {
                equipmentInfo.ProvinceTaxRate = rate;
            }

            equipmentInfo.DealProvince = mainAddressProvinceCode;
            equipmentInfo.CreditAmount = result.Item1.Details?.CreditAmount;
            equipmentInfo.IsAllInfoCompleted = result.Item1.PaymentInfo != null && result.Item1.PrimaryCustomer?.Phones != null && result.Item1.PrimaryCustomer.Phones.Any();
            equipmentInfo.IsApplicantsInfoEditAvailable = result.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;
            equipmentInfo.IsFirstStepAvailable = result.Item1.ContractState != Api.Common.Enumeration.ContractState.Completed;
            equipmentInfo.CreditAmount = result.Item1.Details?.CreditAmount;

            var dealerTier = await _contractServiceAgent.GetDealerTier(contractId);
            equipmentInfo.DealerTier = Mapper.Map<TierViewModel>(dealerTier) ?? new TierViewModel() { RateCards = new List<RateCardViewModel>() };
            equipmentInfo.IsClarityDealer = equipmentInfo.DealerTier?.Name == _clarityProgramTier;

            if(result.Item1.Equipment == null ||
                (result.Item1.Equipment?.RateCardId == null
                    && (result.Item1.Equipment?.NewEquipment?.All(ne => ne?.Cost == null && ne?.MonthlyCost == null) ?? true)))
            {
                equipmentInfo.RateCardValid = true;
                equipmentInfo.IsOldClarityDeal = false;
            }
            else
            {
                equipmentInfo.IsOldClarityDeal = result.Item1.Equipment.IsClarityProgram == null && equipmentInfo.IsClarityDealer;
                equipmentInfo.RateCardValid = result.Item1.Equipment != null &&
                (!result.Item1.Equipment.RateCardId.HasValue || result.Item1.Equipment.RateCardId.Value == 0 || dealerTier.RateCards.Any(x => x.Id == result.Item1.Equipment.RateCardId.Value));
            }

            AddAditionalContractInfo(result.Item1, equipmentInfo);

            if(result.Item1.Comments.Any(x => x.IsCustomerComment == true))
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
            if(contractResult.Item1 == null)
            {
                return new EquipmentInformationViewModel();
            }
            var equipmentInfo = new EquipmentInformationViewModel()
            {
                ContractId = contractId,
            };
            if(contractResult.Item1.Equipment != null)
            {
                equipmentInfo = Mapper.Map<EquipmentInformationViewModel>(contractResult.Item1.Equipment);
                if(!equipmentInfo.NewEquipment.Any())
                {
                    equipmentInfo.NewEquipment = null;
                }
                if(!equipmentInfo.ExistingEquipment.Any())
                {
                    equipmentInfo.ExistingEquipment = null;
                }
            }
            equipmentInfo.Notes = contractResult.Item1.Details?.Notes;

            var mainAddressProvince = contractResult.Item1.PrimaryCustomer.Locations
                .FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State.ToProvinceCode();

            if(mainAddressProvince != null)
            {
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(mainAddressProvince)).Item1;
                if(rate != null)
                { equipmentInfo.ProvinceTaxRate = rate; }
            }

            equipmentInfo.CreditAmount = contractResult.Item1.Details?.CreditAmount;
            equipmentInfo.IsAllInfoCompleted = contractResult.Item1.PaymentInfo != null && contractResult.Item1.PrimaryCustomer?.Phones != null && contractResult.Item1.PrimaryCustomer.Phones.Any();
            equipmentInfo.IsApplicantsInfoEditAvailable = contractResult.Item1.ContractState < Api.Common.Enumeration.ContractState.Completed;
            if(!equipmentInfo.RequestedTerm.HasValue)
            {
                equipmentInfo.RequestedTerm = 120;
            }
            return equipmentInfo;
        }

        public async Task<SummaryAndConfirmationViewModel> GetSummaryAndConfirmationAsync(int contractId, ContractDTO contract = null)
        {
            var summaryAndConfirmation = new SummaryAndConfirmationViewModel();
            var contractResult = contract ?? (await _contractServiceAgent.GetContract(contractId))?.Item1;
            if(contractResult == null)
            {
                return summaryAndConfirmation;
            }

            var dealerTier = await _contractServiceAgent.GetDealerTier(contractId);
            summaryAndConfirmation.DealerTier = Mapper.Map<TierViewModel>(dealerTier);
            summaryAndConfirmation.RateCardValid = !(contractResult.Equipment?.RateCardId.HasValue ?? false) ||
                                                   contractResult.Equipment.RateCardId.Value == 0 ||
                                                   dealerTier.RateCards.Any(
                                                           x => x.Id == contractResult.Equipment.RateCardId.Value);
            var isOldClarityDeal = contractResult.Equipment?.IsClarityProgram == null && dealerTier.Name == _clarityProgramTier;
            summaryAndConfirmation.IsOldClarityDeal = isOldClarityDeal;

            summaryAndConfirmation.IsClarityDealer = dealerTier.Name == _clarityProgramTier;

            await MapSummary(summaryAndConfirmation, contractResult, contractId);

            //correction for value of a deal
            if(summaryAndConfirmation.EquipmentInfo != null)
            {
                summaryAndConfirmation.EquipmentInfo.ValueOfDeal =
                    (double?)GetPaymentSummary(contractResult, (decimal?)summaryAndConfirmation.ProvinceTaxRate?.Rate,
                        summaryAndConfirmation.IsClarityDealer, isOldClarityDeal)?.TotalAmountFinanced
                    ?? summaryAndConfirmation.EquipmentInfo?.ValueOfDeal;
            }

            return summaryAndConfirmation;
        }

        public PaymentSummary GetPaymentSummary(ContractDTO contract, decimal? taxRate, bool? isClarity, bool isOldClarityDeal)
        {
            PaymentSummary paymentSummary = new PaymentSummary();

            if(contract?.Equipment != null)
            {
                if(contract.Equipment.AgreementType == AgreementType.LoanApplication)
                {
                    var priceOfEquipment = 0.0m;
                    if(isClarity == true)
                    {
                        if(isOldClarityDeal)
                        {
                            priceOfEquipment = contract.Equipment?.NewEquipment?.Sum(x => x.Cost) ?? 0;
                        }
                        else
                        {
                            priceOfEquipment = contract.Equipment?.NewEquipment?.Sum(x => x.MonthlyCost) ?? 0.0m;
                            var packages = contract.Equipment?.InstallationPackages?.Sum(x => x.MonthlyCost) ?? 0.0m;
                            priceOfEquipment += packages;
                        }

                    }
                    else
                    {
                        priceOfEquipment = contract.Equipment?.NewEquipment?.Sum(x => x.Cost) ?? 0;
                    }

                    var loanCalculatorInput = new LoanCalculator.Input
                    {
                        TaxRate = (double?)taxRate ?? 0,
                        LoanTerm = contract.Equipment?.LoanTerm ?? 0,
                        AmortizationTerm = contract.Equipment?.AmortizationTerm ?? 0,
                        PriceOfEquipment = (double)priceOfEquipment,
                        AdminFee = (double)(contract.Equipment?.IsFeePaidByCutomer == true ? (double)(contract.Equipment?.AdminFee ?? 0) : 0.0),
                        DownPayment = (double)(contract.Equipment?.DownPayment ?? 0),
                        CustomerRate = (double)(contract.Equipment?.CustomerRate ?? 0),
                        IsClarity = isClarity,
                        IsOldClarityDeal = isOldClarityDeal
                    };
                    var loanCalculatorOutput = LoanCalculator.Calculate(loanCalculatorInput);
                    paymentSummary.Hst = (decimal)loanCalculatorOutput.Hst;
                    paymentSummary.TotalMonthlyPayment = (decimal)loanCalculatorOutput.TotalMonthlyPayment;
                    paymentSummary.MonthlyPayment = (decimal)loanCalculatorOutput.TotalMonthlyPayment;
                    paymentSummary.TotalAllMonthlyPayment = (decimal)loanCalculatorOutput.TotalAllMonthlyPayments;
                    paymentSummary.TotalAmountFinanced = (decimal)loanCalculatorOutput.TotalAmountFinanced;
                    paymentSummary.LoanDetails = loanCalculatorOutput;

                }
                else
                {
                    paymentSummary.MonthlyPayment = contract.Equipment?.TotalMonthlyPayment;
                    paymentSummary.Hst =
                        (contract.Equipment?.TotalMonthlyPayment ?? 0) * ((taxRate ?? 0.0m) / 100);
                    paymentSummary.TotalMonthlyPayment = (contract.Equipment.TotalMonthlyPayment ?? 0) +
                                                  (contract.Equipment.TotalMonthlyPayment ?? 0) *
                                                  ((taxRate ?? 0.0m) / 100);
                    paymentSummary.TotalAllMonthlyPayment = paymentSummary.TotalMonthlyPayment *
                                                            (contract.Equipment.RequestedTerm ?? 0);
                    paymentSummary.TotalAmountFinanced = paymentSummary.TotalAllMonthlyPayment;
                }
            }
            return paymentSummary;
        }

        public async Task<IList<ContractViewModel>> GetContractsAsync(IEnumerable<int> ids)
        {
            var contractsResult = await _contractServiceAgent.GetContracts(ids);
            if(contractsResult.Item1 == null)
            {
                return new List<ContractViewModel>();
            }
            var contracts = new List<ContractViewModel>();
            foreach(var contract in contractsResult.Item1)
            {
                var contractViewModel = new ContractViewModel();
                await MapContract(contractViewModel, contract, contract.Id);
                if(contractViewModel.EquipmentInfo != null)
                {
                    contractViewModel.EquipmentInfo.ValueOfDeal = contract.Equipment != null && contract.Equipment.ValueOfDeal.HasValue ?
                        (double?)contract.Equipment.ValueOfDeal :
                        null;
                }
                contracts.Add(contractViewModel);
            }
            return contracts;
        }

        public async Task<StandaloneCalculatorViewModel> GetStandaloneCalculatorInfoAsync()
        {
            var model = new StandaloneCalculatorViewModel
            {
                EquipmentTypes = (await _dictionaryServiceAgent.GetEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
                ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1
            };

            var dealerTier = await _contractServiceAgent.GetDealerTier();
            model.DealerTier = dealerTier ?? new TierDTO { RateCards = new List<RateCardDTO>() };
            
            var planDict = new Dictionary<RateCardType, string>
            {
                {RateCardType.FixedRate, Resources.Resources.StandardRate},
                {RateCardType.NoInterest, Resources.Resources.EqualPayments},
                {RateCardType.Deferral, Resources.Resources.Deferral},
                {RateCardType.Custom, Resources.Resources.Custom},
            };

            model.Plans = model.DealerTier.RateCards
                .Select(x => x.CardType)
                .Distinct()
                .Where(c => planDict.ContainsKey(c))
                .Select(card => new KeyValuePair<string, string>(card.ToString(), planDict[card]))
                .ToDictionary(card => card.Key, card => card.Value);

            model.DeferralPeriods = model.DealerTier.RateCards
                .Where(x => x.CardType == RateCardType.Deferral)
                .Select(q => Convert.ToInt32(q.DeferralPeriod))
                .Distinct()
                .Select(x => new KeyValuePair<string, string>(x.ToString(), x + " " + (x == 1 ? Resources.Resources.Month : Resources.Resources.Months)))
                .ToDictionary(s => s.Key, s => s.Value);
            model.RateCardProgramsAvailable = model.DealerTier.RateCards.Any(x => x.CustomerRiskGroup != null);

            if(model.DealerTier != null && model.DealerTier.Id ==
                Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["Amort180RateCardId"]))
            {
                model.TotalAmountFinancedFor180AmortTerm = 4999;
            }
            else
            {
                model.TotalAmountFinancedFor180AmortTerm = 3999;
            }

            model.AdminFee = 0;

            return model;
        }

        public async Task<ContractViewModel> GetContractAsync(int contractId)
        {
            var contractsResult = await _contractServiceAgent.GetContract(contractId);
            if(contractsResult.Item1 == null)
            { return null; }
            var contractViewModel = new ContractViewModel();

            await MapContract(contractViewModel, contractsResult.Item1, contractsResult.Item1.Id);
            return contractViewModel;
        }

        public async Task<ContractEditViewModel> GetContractEditAsync(int contractId)
        {
            var contractsResult = await _contractServiceAgent.GetContract(contractId);

            if(contractsResult == null)
            { return null; }
            var summaryViewModel = await GetSummaryAndConfirmationAsync(contractId, contractsResult.Item1);


            var paymentSummary = GetPaymentSummary(contractsResult.Item1,
                (decimal?)summaryViewModel.ProvinceTaxRate?.Rate, summaryViewModel.IsClarityDealer, summaryViewModel.IsOldClarityDeal);

            var contractEditViewModel = new ContractEditViewModel()
            {
                AdditionalInfo = summaryViewModel.AdditionalInfo,
                ContactAndPaymentInfo = summaryViewModel.ContactAndPaymentInfo,
                BasicInfo = summaryViewModel.BasicInfo,
                EquipmentInfo = summaryViewModel.EquipmentInfo,
                ProvinceTaxRate = summaryViewModel.ProvinceTaxRate,
                LoanCalculatorOutput = summaryViewModel.LoanCalculatorOutput,
                PaymentSummary = paymentSummary,
                Notes = summaryViewModel.Notes,
                IsClarityDealer = summaryViewModel.IsClarityDealer,
                IsOldClarityDeal = summaryViewModel.IsOldClarityDeal
            };

            if(contractsResult.Item1.Comments != null)
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

            contractEditViewModel.UploadDocumentsInfo =
                    new UploadDocumentsViewModel
                    {
                        ExistingDocuments =
                                    Mapper.Map<List<ExistingDocument>>(contractsResult
                                            .Item1.Documents),
                        DocumentsForUpload = new List<DocumentForUpload>()
                    };
            var docTypes = await _dictionaryServiceAgent.GetStateDocumentTypes(summaryViewModel.ProvinceTaxRate.Province);
            if(docTypes?.Item1 != null)
            {

                contractEditViewModel.UploadDocumentsInfo.MandatoryDocumentTypes = docTypes.Item1.Where(x => x.IsMandatory).Select(d => d.Id).ToList();
                var otherDoc = docTypes.Item1.SingleOrDefault(x => x.Id == (int)DocumentTemplateType.Other);
                if(otherDoc != null)
                {
                    docTypes.Item1.RemoveAt(docTypes.Item1.IndexOf(otherDoc));
                    docTypes.Item1.Add(otherDoc);
                }
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
            contractEditViewModel.SendEmails = new SendEmailsViewModel
            {
                ContractId = contractId,
                HomeOwnerFullName = $"{contractsResult.Item1.PrimaryCustomer?.FirstName} {contractsResult.Item1.PrimaryCustomer.LastName}",
                HomeOwnerId = contractsResult.Item1.PrimaryCustomer.Id,
                BorrowerEmail = contractsResult.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                contractsResult.Item1.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress,
                SalesRep = contractsResult.Item1.Equipment?.SalesRep,
                AdditionalApplicantsEmails = contractsResult.Item1.SecondaryCustomers.Select(c =>
                new CustomerEmail
                {
                    CustomerId = c.Id,
                    CustomerName = $"{c.FirstName} {c.LastName}",
                    Email = c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                        c.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress
                }).ToArray(),
                AgreementType = contractsResult.Item1.Equipment?.AgreementType.ConvertTo<Models.Enumeration.AgreementType>() ?? Models.Enumeration.AgreementType.LoanApplication
            };
            contractEditViewModel.ESignature = MapESignature(contractsResult.Item1);
            return contractEditViewModel;
        }

        public async Task MapBasicInfo(BasicInfoViewModel basicInfo, ContractDTO contract)
        {
            basicInfo.HomeOwner = Mapper.Map<ApplicantPersonalInfo>(contract.PrimaryCustomer);
            basicInfo.AdditionalApplicants = Mapper.Map<List<ApplicantPersonalInfo>>(contract.SecondaryCustomers);

            basicInfo.SubmittingDealerId = contract.ExternalSubDealerId ?? contract.DealerId;
            basicInfo.SubDealers = new List<SubDealer>();
            var dealerInfo = await _dictionaryServiceAgent.GetDealerInfo();
            if((dealerInfo?.SubDealers?.Any() ?? false) || (dealerInfo?.UdfSubDealers?.Any() ?? false))
            {
                var mainDealer = new SubDealer()
                {
                    Id = dealerInfo.Id,
                    DisplayName = Resources.Resources.OnMyBehalf
                };
                basicInfo.SubDealers.Add(mainDealer);
                if(dealerInfo.SubDealers?.Any() ?? false)
                {
                    basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.SubDealers));
                }
                if(dealerInfo.UdfSubDealers?.Any() ?? false)
                {
                    basicInfo.SubDealers.AddRange(Mapper.Map<IList<SubDealer>>(dealerInfo.UdfSubDealers));
                }
            }
            basicInfo.ContractState = contract.ContractState;
            basicInfo.ContractWasDeclined = contract.WasDeclined ?? false;

            if(contract.PrimaryCustomer?.EmploymentInfo != null)
            {
                basicInfo.HomeOwner.EmploymentInformation = Mapper.Map<EmploymentInformationViewModel>(contract.PrimaryCustomer.EmploymentInfo);
            }
            foreach(var additional in basicInfo.AdditionalApplicants)
            {
                var secondary = contract.SecondaryCustomers?.FirstOrDefault(s => s.Id == additional.CustomerId);
                if(secondary?.EmploymentInfo != null)
                {
                    additional.EmploymentInformation = Mapper.Map<EmploymentInformationViewModel>(secondary.EmploymentInfo);
                }
            }
        }

        public void MapContactAndPaymentInfo(ContactAndPaymentInfoViewModel contactAndPaymentInfo, ContractDTO contract)
        {
            contactAndPaymentInfo.PaymentInfo = Mapper.Map<PaymentInfoViewModel>(
                    contract.PaymentInfo);
            contactAndPaymentInfo.HomeOwnerContactInfo = Mapper.Map<ContactInfoViewModel>(
                    contract.PrimaryCustomer);
            contactAndPaymentInfo.HouseSize = contract.Details.HouseSize;
            contactAndPaymentInfo.CoBorrowersContactInfo = Mapper.Map<List<ContactInfoViewModel>>(contract.SecondaryCustomers);

            //DEAL-3471 
            //Adding other payments to Enbridge gas bills is not possible outside Ontario province, therefore we should block this option for customers from other provinces.
            var mainAddressProvinceCode = contract.PrimaryCustomer.Locations.First(x => x.AddressType == AddressType.MainAddress).State.ToProvinceCode();
            contactAndPaymentInfo.IsAllPaymentTypesAvailable = mainAddressProvinceCode == ContractProvince.ON.ToString();
        }

        public async Task<IList<Alert>> UpdateContractAsync(BasicInfoViewModel basicInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = basicInfo.ContractId ?? 0,
                LeadSource = _leadSource
            };
            if(!string.IsNullOrEmpty(basicInfo.SubmittingDealerId))
            {
                //check dealer for update
                var dealerInfo = await _dictionaryServiceAgent.GetDealerInfo();
                var usd = dealerInfo?.UdfSubDealers?.FirstOrDefault(usdl => usdl.SubmissionValue == basicInfo.SubmittingDealerId);
                if(usd != null)
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
            contractData.PrimaryCustomer = Mapper.Map<CustomerDTO>(basicInfo.HomeOwner);
            contractData.PrimaryCustomer.Locations = new List<LocationDTO>();
            var mainAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.AddressInformation);
            mainAddress.AddressType = AddressType.MainAddress;
            contractData.PrimaryCustomer.Locations.Add(mainAddress);
            if(basicInfo.HomeOwner.MailingAddressInformation != null)
            {
                var mailAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.MailingAddressInformation);
                mailAddress.AddressType = AddressType.MailAddress;
                contractData.PrimaryCustomer.Locations.Add(mailAddress);
            }
            if(basicInfo.HomeOwner.PreviousAddressInformation != null)
            {
                var previousAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.PreviousAddressInformation);
                previousAddress.AddressType = AddressType.PreviousAddress;
                contractData.PrimaryCustomer.Locations.Add(previousAddress);
            }

            var isQuebec = basicInfo.HomeOwner.AddressInformation.Province.ToUpper() == "QC";

            if(isQuebec)
            {
                var emp = Mapper.Map<EmploymentInfoDTO>(basicInfo.HomeOwner.EmploymentInformation);
                contractData.PrimaryCustomer.EmploymentInfo = emp;
            }

            contractData.SecondaryCustomers = new List<CustomerDTO>();
            basicInfo.AdditionalApplicants?.ForEach(a =>
            {
                var customer = Mapper.Map<CustomerDTO>(a);
                customer.Locations = new List<LocationDTO>();
                if(a.AddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(a.AddressInformation);
                    mailAddress.AddressType = AddressType.MainAddress;
                    customer.Locations.Add(mailAddress);
                }
                if(a.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(a.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customer.Locations.Add(previousAddress);
                }
                if(isQuebec)
                {
                    customer.EmploymentInfo = Mapper.Map<EmploymentInfoDTO>(a.EmploymentInformation);
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
                LeadSource = _leadSource,
                Equipment = Mapper.Map<EquipmentInfoDTO>(equipmnetInfo),
                Details = Mapper.Map<ContractDetailsDTO>(equipmnetInfo),
                SalesRepInfo = Mapper.Map<ContractSalesRepInfoDTO>(equipmnetInfo.SalesRepInformation)
            };

            var existingEquipment = Mapper.Map<List<ExistingEquipmentDTO>>(equipmnetInfo.ExistingEquipment);
            contractData.Equipment.ExistingEquipment = existingEquipment ?? new List<ExistingEquipmentDTO>();
            var installationPackeges = Mapper.Map<List<InstallationPackageDTO>>(equipmnetInfo.InstallationPackages);
            contractData.Equipment.InstallationPackages = installationPackeges ?? new List<InstallationPackageDTO>();

            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(EquipmentInformationViewModel equipmnetInfo)
        {
            var contractData = new ContractDataDTO
            {
                Id = equipmnetInfo.ContractId ?? 0,
                LeadSource = _leadSource,
                Equipment = Mapper.Map<EquipmentInfoDTO>(equipmnetInfo)
            };

            if(equipmnetInfo.FullUpdate && equipmnetInfo.ExistingEquipment == null)
            {
                contractData.Equipment.ExistingEquipment = new List<ExistingEquipmentDTO>();
            }

            return await _contractServiceAgent.UpdateContractData(contractData);
        }

        public async Task<IList<Alert>> UpdateContractAsync(ContactAndPaymentInfoViewModel contactAndPaymentInfo)
        {
            var alerts = new List<Alert>();

            var contractData = new ContractDataDTO
            {
                Id = contactAndPaymentInfo.ContractId ?? 0,
                LeadSource = _leadSource
            };
            List<CustomerDataDTO> customers = new List<CustomerDataDTO>();
            if(contactAndPaymentInfo.HomeOwnerContactInfo != null)
            {
                customers.Add(Mapper.Map<CustomerDataDTO>(contactAndPaymentInfo.HomeOwnerContactInfo));
            }
            if(contactAndPaymentInfo.CoBorrowersContactInfo?.Any() ?? false)
            {
                contactAndPaymentInfo.CoBorrowersContactInfo.ForEach(cnt =>
                    customers.Add(Mapper.Map<CustomerDataDTO>(cnt)));
            }

            if(customers.Any())
            {
                customers.ForEach(c =>
                {
                    c.ContractId = contactAndPaymentInfo.ContractId;
                    c.LeadSource = _leadSource;
                });
                alerts.AddRange(await _contractServiceAgent.UpdateCustomerData(customers.ToArray()));
            }

            //if (contactAndPaymentInfo.HouseSize.HasValue)
            //{
            //    contractData.Details = new ContractDetailsDTO()
            //    {
            //        HouseSize = contactAndPaymentInfo.HouseSize
            //    };
            //}

            if(contactAndPaymentInfo.PaymentInfo != null)
            {
                var paymentInfo = Mapper.Map<PaymentInfoDTO>(contactAndPaymentInfo.PaymentInfo);
                contractData.PaymentInfo = paymentInfo;
            }
            alerts.AddRange(await _contractServiceAgent.UpdateContractData(contractData));
            return alerts;
        }

        public async Task<IList<Alert>> UpdateApplicants(ApplicantsViewModel basicInfo)
        {
            var customers = new List<CustomerDataDTO>();
            if(basicInfo.HomeOwner != null)
            {
                var customerDto = Mapper.Map<CustomerDataDTO>(basicInfo.HomeOwner);
                customerDto.Locations = new List<LocationDTO>();
                var mainAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.AddressInformation);
                mainAddress.AddressType = AddressType.MainAddress;
                customerDto.Locations.Add(mainAddress);
                if(basicInfo.HomeOwner.MailingAddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.MailingAddressInformation);
                    mailAddress.AddressType = AddressType.MailAddress;
                    customerDto.Locations.Add(mailAddress);
                }
                if(basicInfo.HomeOwner.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(basicInfo.HomeOwner.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customerDto.Locations.Add(previousAddress);
                }
                customers.Add(customerDto);
            }
            basicInfo.AdditionalApplicants?.ForEach(applicant =>
            {
                var customerDto = Mapper.Map<CustomerDataDTO>(applicant);
                customerDto.Locations = new List<LocationDTO>();
                if(applicant.AddressInformation != null)
                {
                    var mainAddress = Mapper.Map<LocationDTO>(applicant.AddressInformation);
                    mainAddress.AddressType = AddressType.MainAddress;
                    customerDto.Locations.Add(mainAddress);
                }
                if(applicant.MailingAddressInformation != null)
                {
                    var mailAddress = Mapper.Map<LocationDTO>(applicant.MailingAddressInformation);
                    mailAddress.AddressType = AddressType.MailAddress;
                    customerDto.Locations.Add(mailAddress);
                }
                if(applicant.PreviousAddressInformation != null)
                {
                    var previousAddress = Mapper.Map<LocationDTO>(applicant.PreviousAddressInformation);
                    previousAddress.AddressType = AddressType.PreviousAddress;
                    customerDto.Locations.Add(previousAddress);
                }
                customers.Add(customerDto);
            });
            customers.ForEach(c =>
            {
                c.ContractId = basicInfo.ContractId;
                c.LeadSource = _leadSource;
            });

            return await _contractServiceAgent.UpdateCustomerData(customers.ToArray());
        }

        public async Task<Tuple<int?, IList<Alert>>> CreateNewCustomerContract(int contractId)
        {
            int? newContractId = null;
            var alerts = new List<Alert>();

            var contractRes = await _contractServiceAgent.GetContract(contractId);
            if(contractRes.Item2.Any())
            {
                alerts.AddRange(contractRes.Item2);
            }
            if(contractRes.Item1 != null && contractRes.Item2.All(a => a.Type != AlertType.Error))
            {
                var customer = contractRes.Item1.PrimaryCustomer;
                customer.Id = 0;

                var newContractRes = await _contractServiceAgent.CreateContract();
                if(newContractRes.Item2.Any())
                {
                    alerts.AddRange(newContractRes.Item2);
                }
                if(newContractRes.Item1 != null && newContractRes.Item2.All(a => a.Type != AlertType.Error))
                {
                    newContractId = newContractRes.Item1.Id;
                    var contractData = new ContractDataDTO()
                    {
                        Id = newContractRes.Item1.Id,
                        PrimaryCustomer = customer,
                        LeadSource = _leadSource
                    };

                    if(contractRes.Item1.PaymentInfo != null)
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
                            TransitNumber = contractRes.Item1.PaymentInfo.TransitNumber,
                        };
                    }

                    var updateRes = await _contractServiceAgent.UpdateContractData(contractData);
                    if(updateRes.Any())
                    {
                        alerts.AddRange(updateRes);
                    }

                    var updatedContractRes = await _contractServiceAgent.GetContract(newContractId.Value);
                    if(updatedContractRes.Item2.Any())
                    {
                        alerts.AddRange(updatedContractRes.Item2);
                    }
                    if(updatedContractRes.Item1?.PrimaryCustomer != null && updatedContractRes.Item2.All(a => a.Type != AlertType.Error))
                    {
                        var updatedCustomer = new CustomerDataDTO()
                        {
                            Id = updatedContractRes.Item1.PrimaryCustomer.Id,
                            ContractId = newContractId,
                            Emails = contractRes.Item1.PrimaryCustomer.Emails,
                            Phones = contractRes.Item1.PrimaryCustomer.Phones,
                            LeadSource = _leadSource
                        };
                        await _contractServiceAgent.UpdateCustomerData(new[] { updatedCustomer });
                    }
                }
            }

            return new Tuple<int?, IList<Alert>>(newContractId, alerts);
        }

        public async Task<bool> CheckRateCard(int contractId, int? rateCardId)
        {
            Tuple<ContractDTO, IList<Alert>> result = await _contractServiceAgent.GetContract(contractId);
            var dealerTier = await _contractServiceAgent.GetDealerTier(contractId);

            if(result.Item1.Equipment == null)
            {
                return true;
            }
            if(rateCardId.HasValue)
            {
                if(rateCardId.Value == 0)
                {
                    return true;
                }
                return result.Item1.Equipment != null &&
                       (!result.Item1.Equipment.RateCardId.HasValue || result.Item1.Equipment.RateCardId.Value == 0 ||
                        dealerTier.RateCards.Any(x => x.Id == rateCardId.Value));
            }
            return result.Item1.Equipment != null &&
                   (!result.Item1.Equipment.RateCardId.HasValue || result.Item1.Equipment.RateCardId.Value == 0 ||
                    dealerTier.RateCards.Any(x => x.Id == result.Item1.Equipment.RateCardId.Value));
        }

        private async Task MapSummary(SummaryAndConfirmationViewModel summary, ContractDTO contract, int contractId)
        {
            summary.BasicInfo = new BasicInfoViewModel { ContractId = contractId };
            await MapBasicInfo(summary.BasicInfo, contract);
            summary.EquipmentInfo = Mapper.Map<EquipmentInformationViewModel>(contract.Equipment);

            if(summary.EquipmentInfo != null)
            {
                summary.EquipmentInfo.CreditAmount = contract.Details?.CreditAmount;
                summary.EquipmentInfo.IsApplicantsInfoEditAvailable = contract.ContractState <= Api.Common.Enumeration.ContractState.Completed;
                summary.EquipmentInfo.IsEditAvailable = contract.ContractState < Api.Common.Enumeration.ContractState.Completed;
                summary.EquipmentInfo.IsFirstStepAvailable = contract.ContractState != Api.Common.Enumeration.ContractState.Completed;
                summary.EquipmentInfo.Notes = contract.Details?.Notes;
                summary.EquipmentInfo.IsFeePaidByCutomer = contract.Equipment.IsFeePaidByCutomer;

                var dealerTier = await _contractServiceAgent.GetDealerTier();
                var rateCard = dealerTier.RateCards.FirstOrDefault(rc => rc.Id == contract.Equipment.RateCardId);

                summary.EquipmentInfo.CustomerRiskGroup = rateCard?.CustomerRiskGroup != null ?
                    new CustomerRiskGroupViewModel { GroupName = rateCard.CustomerRiskGroup.GroupName } :
                    null;
            }
            summary.Notes = contract.Details?.Notes;
            summary.AdditionalInfo = new AdditionalInfoViewModel();

            summary.AdditionalInfo.SalesRepRole = new List<string>();

            if(contract.SalesRepInfo?.InitiatedContact == true)
            {
                summary.AdditionalInfo.SalesRepRole.Add(Resources.Resources.InitiatedContract);
            }
            if(contract.SalesRepInfo?.NegotiatedAgreement == true)
            {
                summary.AdditionalInfo.SalesRepRole.Add(Resources.Resources.NegotiatedAgreement);
            }
            if(contract.SalesRepInfo?.ConcludedAgreement == true)
            {
                summary.AdditionalInfo.SalesRepRole.Add(Resources.Resources.ConcludedAgreement);
            }

            if(contract?.Comments.Any(x => x.IsCustomerComment == true) == true)
            {
                var comments = contract.Comments
                    .Where(x => x.IsCustomerComment == true)
                    .Select(q => q.Text)
                    .ToList();

                summary.AdditionalInfo.CustomerComments = comments;
            }

            summary.ContactAndPaymentInfo = new ContactAndPaymentInfoViewModel { ContractId = contractId };
            MapContactAndPaymentInfo(summary.ContactAndPaymentInfo, contract);
            if(summary.BasicInfo.HomeOwner?.AddressInformation != null)
            {
                var rate = (await _dictionaryServiceAgent.GetProvinceTaxRate(summary.BasicInfo.HomeOwner.AddressInformation.Province.ToProvinceCode())).Item1;
                if(rate != null)
                { summary.ProvinceTaxRate = rate; }
            }

            summary.AdditionalInfo.ContractState = contract.ContractState.ConvertTo<ContractState>();
            summary.AdditionalInfo.Status = contract.Details?.LocalizedStatus ?? contract.ContractState.GetEnumDescription();
            summary.AdditionalInfo.LastUpdateTime = contract.LastUpdateTime;
            summary.AdditionalInfo.TransactionId = contract.Details?.TransactionId;
            summary.AdditionalInfo.IsCreatedByCustomer = contract.IsCreatedByCustomer ?? false;
            if(contract.Equipment != null && contract.Equipment.AgreementType == AgreementType.LoanApplication)
            {
                var paymentSummary = GetPaymentSummary(contract,
                    (decimal?)summary.ProvinceTaxRate?.Rate ?? 0.0m, summary.IsClarityDealer, summary.IsOldClarityDeal);
                summary.LoanCalculatorOutput = paymentSummary.LoanDetails;

                if(summary.IsClarityDealer && !summary.IsOldClarityDeal)
                {
                    double dpWithTax = contract.Equipment == null || !contract.Equipment.DownPayment.HasValue ? 0.0 :
                        (double)contract.Equipment?.DownPayment * PortalConstants.ClarityFactor /
                                       (1.0 + (summary.ProvinceTaxRate?.Rate ?? 0) / 100);
                    var total = summary.EquipmentInfo?.NewEquipment.Sum(ne => ne.MonthlyCost) + (double)summary.EquipmentInfo?.InstallationPackages?.Sum(i => i.MonthlyCost);

                    summary.EquipmentInfo?.NewEquipment?.ForEach(ne =>
                    {
                        double lessDp = ne.MonthlyCost.HasValue ? (double)(ne.MonthlyCost.Value - dpWithTax / total * ne.MonthlyCost.Value) : 0.0;

                        ne.MonthlyCostLessDP = Math.Round(lessDp, 2);
                    });

                    summary.EquipmentInfo?.InstallationPackages?.ForEach(ne =>
                    {
                        double lessDp = ne.MonthlyCost.HasValue ? (double)(ne.MonthlyCost.Value - dpWithTax / total * ne.MonthlyCost.Value) : 0.0;

                        ne.MonthlyCostLessDP = Math.Round(lessDp, 2);
                    });
                }
            }
        }

        public async Task<ESignatureViewModel> GetContractSignatureStatus(int contractId)
        {
            var contractsResult = await _contractServiceAgent.GetContract(contractId);

            return contractsResult == null ? null : MapESignature(contractsResult.Item1);
        }

        private async Task MapContract(ContractViewModel contractViewModel, ContractDTO contract, int contractId)
        {
            var summaryViewModel = await GetSummaryAndConfirmationAsync(contractId, contract);

            var paymentSummary = GetPaymentSummary(contract,
                (decimal?)summaryViewModel.ProvinceTaxRate?.Rate, summaryViewModel.IsClarityDealer, summaryViewModel.IsOldClarityDeal);

            contractViewModel.AdditionalInfo = summaryViewModel.AdditionalInfo;
            contractViewModel.ContactAndPaymentInfo = summaryViewModel.ContactAndPaymentInfo;
            contractViewModel.BasicInfo = summaryViewModel.BasicInfo;
            contractViewModel.EquipmentInfo = summaryViewModel.EquipmentInfo;
            contractViewModel.ProvinceTaxRate = summaryViewModel.ProvinceTaxRate;
            contractViewModel.LoanCalculatorOutput = summaryViewModel.LoanCalculatorOutput;
            contractViewModel.PaymentSummary = paymentSummary;
            contractViewModel.IsClarityDealer = summaryViewModel.IsClarityDealer;
            contractViewModel.IsOldClarityDeal = summaryViewModel.IsOldClarityDeal;

            contractViewModel.UploadDocumentsInfo =
                    new UploadDocumentsViewModel
                    {
                        ExistingDocuments =
                                    Mapper.Map<List<ExistingDocument>>(contract.Documents),
                        DocumentsForUpload = new List<DocumentForUpload>()
                    };

            var docTypes = await _dictionaryServiceAgent.GetStateDocumentTypes(summaryViewModel.ProvinceTaxRate.Province);
            if(docTypes?.Item1 != null)
            {
                contractViewModel.UploadDocumentsInfo.MandatoryDocumentTypes = docTypes.Item1.Where(x => x.IsMandatory).Select(d => d.Id).ToList();
                contractViewModel.UploadDocumentsInfo.DocumentTypes = docTypes.Item1.Select(d => new SelectListItem()
                {
                    Value = d.Id.ToString(),
                    Text = d.Description
                }).ToList();
            }

            if(contract.Comments != null && contract.Comments.Any(x => x.IsCustomerComment == true))
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

            if(contract.Equipment != null)
            {
                equipmentInfo.PrefferedInstallDate = contract.Equipment.EstimatedInstallationDate;
                equipmentInfo.SalesRepInformation.SalesRep = contract.Equipment.SalesRep;
                equipmentInfo.ExistingEquipment = Mapper.Map<List<ExistingEquipmentInformation>>(contract.Equipment.ExistingEquipment);
            }
        }

        private ESignatureViewModel MapESignature(ContractDTO contract)
        {
            var borrower = contract.Signers.FirstOrDefault(s => s.SignerType == SignatureRole.HomeOwner);
            var salesRep = contract.Signers.FirstOrDefault(s => s.SignerType == SignatureRole.Dealer);
            var additionSigners = contract.Signers.Where(s => s.SignerType == SignatureRole.AdditionalApplicant);
            var model = new ESignatureViewModel
            {
                ContractId = contract.Id,
                HomeOwnerId = contract.PrimaryCustomer.Id,
                Status = contract.Details.SignatureStatus,
                Signers = new List<SignerViewModel>()
            };

            model.Signers.Add(
                new SignerViewModel
                {
                    Id = borrower?.Id ?? 0,
                    CustomerId = borrower?.CustomerId ?? contract.PrimaryCustomer.Id,
                    FirstName = !string.IsNullOrEmpty(borrower?.FirstName) ? borrower.FirstName : contract.PrimaryCustomer?.FirstName,
                    LastName = !string.IsNullOrEmpty(borrower?.LastName) ? borrower.LastName : contract.PrimaryCustomer?.LastName,
                    Email = borrower != null ? borrower.EmailAddress : contract.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                                                       contract.PrimaryCustomer.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress,
                    StatusLastUpdateTime = borrower?.StatusLastUpdateTime?.TryConvertToLocalUserDate(),
                    Comment = borrower?.Comment,
                    SignatureStatus = borrower != null ? borrower.SignatureStatus : contract.Details.SignatureStatus,
                    Role = borrower?.SignerType ?? SignatureRole.HomeOwner
                });

            contract.SecondaryCustomers.ForEach(s =>
            {
                var signer = additionSigners.FirstOrDefault(ads => ads.CustomerId == s.Id);
                model.Signers.Add(new SignerViewModel()
                {
                    Id = signer?.Id ?? 0,
                    CustomerId = s.Id,
                    Comment = signer?.Comment,
                    Email = signer?.EmailAddress ?? s.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Notification)?.EmailAddress ??
                                s.Emails?.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress,
                    FirstName = !string.IsNullOrEmpty(signer?.FirstName) ? signer.FirstName : s.FirstName,
                    LastName = !string.IsNullOrEmpty(signer?.LastName) ? signer.LastName : s.LastName,
                    SignatureStatus = signer?.SignatureStatus,
                    StatusLastUpdateTime = signer?.StatusLastUpdateTime?.TryConvertToLocalUserDate(),
                    Role = SignatureRole.AdditionalApplicant
                });
            });

            model.Signers.Add(new SignerViewModel
            {
                Id = salesRep?.Id ?? 0,
                CustomerId = null,
                FirstName = salesRep?.FirstName,
                LastName = !string.IsNullOrEmpty(salesRep?.LastName) ? salesRep.LastName : contract.Equipment?.SalesRep,
                Email = salesRep?.EmailAddress,
                Comment = salesRep?.Comment,
                StatusLastUpdateTime = salesRep?.StatusLastUpdateTime?.TryConvertToLocalUserDate(),
                SignatureStatus = salesRep != null ? salesRep.SignatureStatus : contract.Details.SignatureStatus,
                Role = salesRep?.SignerType ?? SignatureRole.Dealer
            });
            return model;
        }
    }
}

