﻿using System;
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
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
using Microsoft.Practices.ObjectBuilder2;

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
        private readonly IMailService _mailService;
        private readonly IAppConfiguration _configuration;

        public DealerService(IDealerRepository dealerRepository, IDealerOnboardingRepository dealerOnboardingRepository, 
            IAspireService aspireService, ILoggingService loggingService, IUnitOfWork unitOfWork, IContractRepository contractRepository, IMailService mailService,
            IAppConfiguration configuration)
        {
            _dealerRepository = dealerRepository;
            _dealerOnboardingRepository = dealerOnboardingRepository;
            _aspireService = aspireService;
            _loggingService = loggingService;
            _unitOfWork = unitOfWork;
            _contractRepository = contractRepository;
            _mailService = mailService;
            _configuration = configuration;
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
            mappedInfo.SalesRepLink = _contractRepository.GetDealer(dealerInfo.ParentSalesRepId).OnboardingLink;

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
            if (dealerInfo == null)
            {
                throw new ArgumentNullException(nameof(dealerInfo));
            }

            var alerts = new List<Alert>();
            DealerInfoKeyDTO resultKey = null;
            try
            {
                var mappedInfo = Mapper.Map<DealerInfo>(dealerInfo);
                mappedInfo.ParentSalesRepId = mappedInfo.ParentSalesRepId ??
                                              _dealerRepository.GetUserIdByOnboardingLink(dealerInfo.SalesRepLink);
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
            if (dealerInfo == null)
            {
                throw new ArgumentNullException(nameof(dealerInfo));
            }

            var alerts = new List<Alert>();
            try
            {
                //update draft in a database as we should have it with required documents 
                var mappedInfo = Mapper.Map<DealerInfo>(dealerInfo);
                mappedInfo.ParentSalesRepId = mappedInfo.ParentSalesRepId ??
                                              _dealerRepository.GetUserIdByOnboardingLink(dealerInfo.SalesRepLink);
                var updatedInfo = _dealerOnboardingRepository.AddOrUpdateDealerInfo(mappedInfo);
                _unitOfWork.Save();
                //submit form to Aspire                                             
                var reSubmit = updatedInfo.SentToAspire;
                var submitResult = await _aspireService.SubmitDealerOnboarding(updatedInfo.Id);
                if (submitResult?.Any() ?? false)
                {
                    alerts.AddRange(submitResult);
                }
                if (submitResult?.Any(r => r.Type == AlertType.Error) == true)
                {
                    //notify dealnet here about failed upload to Aspire
                    var errorMsg = string.Concat(submitResult.Where(x => x.Type == AlertType.Error).Select(r => r.Header + ": " + r.Message).ToArray());
                    await _mailService.SendProblemsWithSubmittingOnboarding(errorMsg, updatedInfo.Id, mappedInfo.AccessKey);
                }
                //upload required documents
                UploadOnboardingDocuments(updatedInfo.Id, reSubmit ? updatedInfo.Status : _configuration.GetSetting(WebConfigKeys.ONBOARDING_INIT_STATUS_KEY));
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

        public async Task<IList<Alert>> SendDealerOnboardingDraftLink(DraftLinkDTO link)
        {
            if (string.IsNullOrEmpty(link.AccessKey))
            {
                throw new ArgumentNullException(nameof(link));
            }

            var alerts = new List<Alert>();
            try
            {
                await _mailService.SendDraftLinkMail(link.AccessKey, link.Email);
            }
            catch (Exception ex)
            {
                var errorMsg = $"Cannot send draf link by email";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.SubmitFailed,
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }
            return alerts;
        }

        public bool CheckOnboardingLink(string dealerLink)
        {
            return _dealerRepository.GetUserIdByOnboardingLink(dealerLink) != null;
        }

        public async Task<Tuple<DealerInfoKeyDTO, IList<Alert>>> AddDocumentToOnboardingForm(RequiredDocumentDTO document)
        {
            var alerts = new List<Alert>();
            DealerInfoKeyDTO resultKey = null;
            try
            {
                var mappedDoc = Mapper.Map<RequiredDocument>(document);
                var updatedDoc = _dealerOnboardingRepository.AddDocumentToDealer(mappedDoc.DealerInfoId, mappedDoc);
                _unitOfWork.Save();                                
                resultKey = new DealerInfoKeyDTO()
                {
                    AccessKey = updatedDoc.DealerInfo?.AccessKey,
                    DealerInfoId = updatedDoc.DealerInfo?.Id ?? 0,
                    ItemId = updatedDoc.Id
                };
                //if form was submitted before, we can upload document to Aspire
                //if (updatedDoc.DealerInfo?.SentToAspire == true &&
                //    !string.IsNullOrEmpty(updatedDoc.DealerInfo?.TransactionId))
                //{
                //    var status = await _aspireService.GetDealStatus(updatedDoc.DealerInfo.TransactionId);
                //    var uAlerts = await _aspireService.UploadOnboardingDocument(updatedDoc.DealerInfo.Id, updatedDoc.Id, !string.IsNullOrEmpty(status) ? status : null);
                //    if (uAlerts?.Any() == true)
                //    {
                //        alerts.AddRange(uAlerts);
                //    }
                //    if (!string.IsNullOrEmpty(status) && updatedDoc.DealerInfo.Status != status)
                //    {
                //        updatedDoc.DealerInfo.Status = status;
                //        _unitOfWork.Save();
                //    }
                //}
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

        public IList<Alert> DeleteDocumentFromOnboardingForm(RequiredDocumentDTO document)
        {
            var alerts = new List<Alert>();
            try
            {
                if (document.DealerInfoId.HasValue)
                {
                    var dealerInfo = _dealerOnboardingRepository.GetDealerInfoById(document.DealerInfoId.Value);
                    if (dealerInfo != null)
                    {
                        _dealerOnboardingRepository.DeleteDocumentFromDealer(document.Id);

                        _unitOfWork.Save();
                    }
                    else
                    {
                        alerts.Add(new Alert()
                        {
                            Header = "Cannot delete document from a dealer onboarding info",
                            Type = AlertType.Error,
                            Message = "Info of this dealer not exists."
                        });
                    }
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Header = "Cannot delete document from a dealer onboarding info",
                        Type = AlertType.Error,
                        Message = "No dealer info id."
                    });
                }
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Header = "Cannot delete document from a dealer onboarding info",
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        public IList<Alert> DeleteDealerOnboardingForm(int dealerInfoId)
        {
            var alerts = new List<Alert>();
            try
            {
                if (_dealerOnboardingRepository.DeleteDealerInfo(dealerInfoId))
                {
                    _unitOfWork.Save();                    
                }                
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Header = "Cannot delete dealer onboarding info",
                    Type = AlertType.Error,
                    Message = ex.ToString()
                });
            }

            return alerts;
        }

        private void UploadOnboardingDocuments(int dealerInfoId, string statusToSend = null)
        {
            var dealerInfo = _dealerOnboardingRepository.GetDealerInfoById(dealerInfoId);
            if (dealerInfo?.RequiredDocuments?.Any(d => d.DocumentBytes != null) == true)
            {
                Task.Run(() =>
                {
                    dealerInfo.RequiredDocuments.Where(d => !d.Uploaded).ForEach(doc =>
                    {
                        _aspireService.UploadOnboardingDocument(dealerInfoId, doc.Id, statusToSend).GetAwaiter().GetResult();                        
                    });                    
                });
            }           
        }
    }

    
}
