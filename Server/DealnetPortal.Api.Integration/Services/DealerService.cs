using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Dealer;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;
        private readonly IDealerOnboardingRepository _dealerOnboardingRepository;
        private readonly IAspireService _aspireService;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;


        public DealerService(IDealerRepository dealerRepository, IDealerOnboardingRepository dealerOnboardingRepository, 
            IAspireService aspireService, ILoggingService loggingService, IUnitOfWork unitOfWork, IContractRepository contractRepository)
        {
            _dealerRepository = dealerRepository;
            _dealerOnboardingRepository = dealerOnboardingRepository;
            _aspireService = aspireService;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
        }

        public DealerProfileDTO GetDealerProfile(string dealerId)
        {
            var profile = _dealerRepository.GetDealerProfile(dealerId);
            return Mapper.Map<DealerProfileDTO>(profile);
        }

        public IList<Alert> UpdateDealerProfile(DealerProfileDTO dealerProfile)
        {
            var alerts = new List<Alert>();

            try
            {
                var profile = Mapper.Map<DealerProfile>(dealerProfile);
                var newProfile = _dealerRepository.UpdateDealerProfile(profile);
                if (newProfile != null)
                {

                    _unitOfWork.Save();
                    var dealer = _contractRepository.GetDealer(newProfile.DealerId);
                    dealer.DealerProfileId = newProfile.Id;
                    _dealerRepository.UpdateDealer(dealer);
                    _unitOfWork.Save();
                }
                else
                {
                    _loggingService.LogError($"Failed to update a dealer profile for [{dealerProfile.DealerId}] dealer");
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Code = ErrorCodes.FailedToUpdateSettings,
                        Message = "Failed to update a dealer profile"
                    });
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to update a dealer profile for [{dealerProfile.DealerId}] dealer", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.FailedToUpdateSettings,
                    Message = "Failed to update a dealer profile"
                });
            }
            return alerts;
        }

        public DealerInfoDTO GetDealerOnboardingForm(string accessKey)
        {
            var dealerInfo = _dealerOnboardingRepository.GetDealerInfoByAccessKey(accessKey);
            var mappedInfo = Mapper.Map<DealerInfoDTO>(dealerInfo);
            return mappedInfo;
        }

        public DealerInfoDTO GetDealerOnboardingForm(int id)
        {
            var dealerInfo = _dealerOnboardingRepository.GetDealerInfoById(id);
            var mappedInfo = Mapper.Map<DealerInfoDTO>(dealerInfo);
            return mappedInfo;
        }

        public Tuple<DealerInfoKeyDTO, IList<Alert>> UpdateDealerOnboardingForm(DealerInfoDTO dealerInfo)
        {
            var alerts = new List<Alert>();
            DealerInfoKeyDTO resultKey = null;
            try
            {
                var mappedInfo = Mapper.Map<DealerInfo>(dealerInfo);
                var updatedInfo = _dealerOnboardingRepository.AddOrUpdateDealerInfo(mappedInfo);
                _unitOfWork.Save();
                resultKey = new DealerInfoKeyDTO()
                {
                    AccessKey = updatedInfo.AccessKey,
                    DealerInfoId = updatedInfo.Id
                };
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Header = "Cannot update dealer onboarding info",
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });                
            }
            return new Tuple<DealerInfoKeyDTO, IList<Alert>>(resultKey, alerts);
        }

        public async Task<IList<Alert>> SubmitDealerOnboardingForm(DealerInfoDTO dealerInfo)
        {
            var alerts = new List<Alert>();
            try
            {
                //update draft in a database as we should have it with required documents 
                var mappedInfo = Mapper.Map<DealerInfo>(dealerInfo);
                var updatedInfo = _dealerOnboardingRepository.AddOrUpdateDealerInfo(mappedInfo);
                _unitOfWork.Save();
                //submit form to Aspire
                var submitResult = await _aspireService.SubmitDealerOnboarding(updatedInfo.Id);
                if (submitResult?.Any() ?? false)
                {
                    alerts.AddRange(submitResult);
                }
            }
            catch (Exception ex)
            {
                var errorMsg = $"Cannot submit dealer onboarding form";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.FailedToUpdateContract,
                    Header = ErrorConstants.SubmitFailed,
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }
            return alerts;
        }

        public Tuple<DealerInfoKeyDTO, IList<Alert>> AddDocumentToOnboardingForm(RequiredDocumentDTO document)
        {
            var alerts = new List<Alert>();
            DealerInfoKeyDTO resultKey = null;
            try
            {
                var mappedDoc = Mapper.Map<RequiredDocument>(document);
                var updatedDoc = _dealerOnboardingRepository.AddDocumentToDealer(mappedDoc.Id, mappedDoc);
                _unitOfWork.Save();
                resultKey = new DealerInfoKeyDTO()
                {
                    AccessKey = updatedDoc.DealerInfo?.AccessKey,
                    DealerInfoId = updatedDoc.DealerInfo?.Id ?? 0,
                    ItemId = updatedDoc.Id
                };
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Header = "Cannot add document to a dealer onboarding info",
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });
            }
            return new Tuple<DealerInfoKeyDTO, IList<Alert>>(resultKey, alerts);
        }
    }

    
}
