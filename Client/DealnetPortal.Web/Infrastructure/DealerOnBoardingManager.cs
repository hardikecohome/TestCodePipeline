using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Models.Scanning;
using DealnetPortal.Web.Models;

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
            var linkDto = new DraftLinkDTO {AccessKey = model.AccessKey, Email = model.Email};
            return await _dealerServiceAgent.SendDealerOnboardingDraftLink(linkDto);
        }

        public async Task<DocumentResponseViewModel> UploadOnboardingDocument(OnboardingDocumentForUpload fileModel)
        {
            byte[] documentBytes;
            using (var reader = new BinaryReader(fileModel.File.InputStream))
            {
                documentBytes = reader.ReadBytes(fileModel.File.ContentLength);
            }

            var model = new RequiredDocumentDTO
            {
                Id = 0,
                DocumentName = fileModel.DocumentName,
                DocumentBytes = documentBytes,
                CreationDate = DateTime.UtcNow,
                DocumentTypeId = fileModel.DocumentTypeId,
                DealerInfoId = fileModel.DealerInfoId
            };

            var result = await _dealerServiceAgent.AddDocumentToOnboardingForm(model);
            var response = new DocumentResponseViewModel();

            if (result.Item2 != null && result.Item2.Any(r => r.Type == AlertType.Error))
            {
                response.IsSuccess = false;
                response.AggregatedError = result.Item2.Select(x => x.Message).Aggregate((x, y) => x + " " + y);
            }

            if (result.Item1 != null)
            {
                response.DealerInfoId = result.Item1.DealerInfoId;
                response.AccessKey = result.Item1.AccessKey;
                response.ItemId = result.Item1.ItemId;
            }
            
            return response;
        }

        public async Task<DocumentResponseViewModel> DeleteOnboardingDocument(OnboardingDocumentForDelete documentForDelete)
        {
            var model = new RequiredDocumentDTO
            {
                Id = documentForDelete.DocumentId,
                DealerInfoId = documentForDelete.DealerInfoId
            };

            var result = await _dealerServiceAgent.DeleteDocumentFromOnboardingForm(model);

            var response = new DocumentResponseViewModel();

            if (result != null && result.Any(r => r.Type == AlertType.Error))
            {
                response.IsSuccess = false;
                response.AggregatedError = result.Select(x => x.Message).Aggregate((x, y) => x + " " + y);
            }

            return response;
        }
    }
}