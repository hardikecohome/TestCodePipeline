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
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Api.Models.Notify;

namespace DealnetPortal.Api.Integration.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMailService _mailService;

        public DealerService(IDealerRepository dealerRepository, ILoggingService loggingService, IUnitOfWork unitOfWork, IContractRepository contractRepository, IMailService mailService)
        {
            _dealerRepository = dealerRepository;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
            _mailService = mailService;
        }

        public DealerProfileDTO GetDealerProfile(string dealerId)
        {
            var profile = _dealerRepository.GetDealerProfile(dealerId);
            return Mapper.Map<DealerProfileDTO>(profile);
        }

        public string GetDealerParentName(string dealerId)
        {
            var parentName = _contractRepository.GetDealer(_dealerRepository.GetParentDealerId(dealerId)?? dealerId).AspireLogin;
            return parentName;
        }

        public IList<Alert> DealerSupportRequestEmail(SupportRequestDTO dealerSupportRequest)
        {
            var alerts = new List<Alert>();

            try
            {
                var result = _mailService.SendSupportRequiredEmail(dealerSupportRequest);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Failed to send Dealer support request for [{dealerSupportRequest.YourName}] dealer with support ID [{dealerSupportRequest.Id}]", ex);
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Code = ErrorCodes.FailedToUpdateSettings,
                    Message = "Failed to send Dealer support request"
                });
            }

            return alerts;
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
    }

    
}
