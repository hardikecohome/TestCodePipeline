﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Scanning;

namespace DealnetPortal.Web.Infrastructure
{
    public class DealerOnBoardingManager : IDealerOnBoardingManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly IDealerServiceAgent _dealerServiceAgent;

        public DealerOnBoardingManager(IDictionaryServiceAgent dictionaryServiceAgent, IDealerServiceAgent dealerServiceAgent)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _dealerServiceAgent = dealerServiceAgent;
        }

        public async Task<DealerOnboardingViewModel> GetNewDealerOnBoardingForm(string onboardingLink)
        {
            var valid = await _dealerServiceAgent.CheckOnboardingLink(onboardingLink);
            return valid ? new DealerOnboardingViewModel
            {
                OnBoardingLink = onboardingLink,
                DictionariesData = new DealerOnboardingDictionariesViewModel
                {
                    ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1
                                                      ?.OrderBy(x => x.Description).ToList(),
                    LicenseDocuments = (await _dictionaryServiceAgent.GetAllLicenseDocuments()).Item1.ToList()
                },
                AdditionalDocuments = new List<AdditionalDocumentViewModel>()
                {
                    new AdditionalDocumentViewModel { ExpiredDate = new DateTime?(DateTime.Today), Number = "123456", NotExpired = false, LicenseTypeId = 3},
                    new AdditionalDocumentViewModel { ExpiredDate = null, Number = "54433", NotExpired = true, LicenseTypeId = 1}
                },
                RequiredDocuments = new List<RequiredDocumentViewModel>()
                {
                    new RequiredDocumentViewModel { Name = "Test.png"},
                    new RequiredDocumentViewModel { Name = "cheque.png"},
                    new RequiredDocumentViewModel { Name = "Test2.jpg"},
                }
            } : null;
        }

        public async Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsync(string accessKey)
        {
            DealerInfoDTO onboardingForm;
            DealerOnboardingViewModel model;
            onboardingForm = await _dealerServiceAgent.GetDealerOnboardingForm(accessKey);
            model = AutoMapper.Mapper.Map<DealerOnboardingViewModel>(onboardingForm);
            if (model != null)
            {
                model.DictionariesData = new DealerOnboardingDictionariesViewModel
                {
                    ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
                    LicenseDocuments = (await _dictionaryServiceAgent.GetAllLicenseDocuments()).Item1.ToList()
                };
            }

            return model;
        }

        public async Task<SaveAndResumeViewModel> SaveDraft(DealerOnboardingViewModel model)
        {
            DealerInfoDTO dto = AutoMapper.Mapper.Map<DealerInfoDTO>(model);
            var result = await _dealerServiceAgent.UpdateDealerOnboardingForm(dto);
            return new SaveAndResumeViewModel
            {
                Id = result.Item1?.DealerInfoId ?? 0,
                AccessKey = result.Item1?.AccessKey ?? model.AccessKey,
                Success = result.Item2 != null ? !result.Item2.Any(a => a.Type == AlertType.Error) : true,
                Alerts = result.Item2,
                Email = model.Owners.Any() && !string.IsNullOrEmpty(model.Owners.First().EmailAddress)
                                        ? model.Owners.First().EmailAddress
                                        : !string.IsNullOrEmpty(model.CompanyInfo.EmailAddress)
                                            ? model.CompanyInfo.EmailAddress
                                            : String.Empty
            };
        }

        public async Task<IList<Alert>> SubmitOnBoarding(DealerOnboardingViewModel model)
        {
            DealerInfoDTO dto = AutoMapper.Mapper.Map<DealerInfoDTO>(model);
            return await _dealerServiceAgent.SubmitDealerOnboardingForm(dto);
        }

        public async Task<IList<Alert>> SendDealerOnboardingDraftLink(SaveAndResumeViewModel model)
        {
            return await _dealerServiceAgent.SendDealerOnboardingDraftLink(model.AccessKey);
        }

        public async Task<IList<Alert>> UploadOnboardingDocument(ScanningRequest scanningRequest, bool isCheque)
        {
            if (isCheque)
            {
                return await _dealerServiceAgent.UploadOnboardingChequeDocument(scanningRequest);
            }
            else
            {
                return await _dealerServiceAgent.UploadOnboardingInsurenceDocument(scanningRequest);
            }
        }
    }
}