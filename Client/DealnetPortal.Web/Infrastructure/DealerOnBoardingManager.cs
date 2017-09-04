using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;

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
            return new DealerOnboardingViewModel
            {
                OnBoardingLink = onboardingLink,
                DictionariesData = new DealerOnboardingDictionariesViewModel
                {
                    ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1
                                                      ?.OrderBy(x => x.Description).ToList(),
                    LicenseDocuments = (await _dictionaryServiceAgent.GetAllLicenseDocuments()).Item1.ToList()
                }
            };
        }

        public async Task<DealerOnboardingViewModel> GetDealerOnBoardingFormAsync(string accessKey)
        {
            DealerInfoDTO onboardingForm;
            DealerOnboardingViewModel model;
            onboardingForm = await _dealerServiceAgent.GetDealerOnboardingForm(accessKey);
            model = AutoMapper.Mapper.Map<DealerOnboardingViewModel>(onboardingForm) ?? new DealerOnboardingViewModel();
            model.DictionariesData = new DealerOnboardingDictionariesViewModel
            {
                ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList(),
                LicenseDocuments = (await _dictionaryServiceAgent.GetAllLicenseDocuments()).Item1.ToList()
            };

            return model;
        }

        public async Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> SaveDraft(DealerOnboardingViewModel model)
        {
            DealerInfoDTO dto = AutoMapper.Mapper.Map<DealerInfoDTO>(model);
            return await _dealerServiceAgent.UpdateDealerOnboardingForm(dto);
        }

        public async Task<IList<Alert>> SubmitOnBoarding(DealerOnboardingViewModel model)
        {
            DealerInfoDTO dto = AutoMapper.Mapper.Map<DealerInfoDTO>(model);
            return await _dealerServiceAgent.SubmitDealerOnboardingForm(dto);
        }

        public async Task<IList<Alert>> SendEmail(SaveAndResumeViewModel model)
        {
            return new List<Alert>();
        }
    }
}