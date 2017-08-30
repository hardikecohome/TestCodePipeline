using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Integration.Utility;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Dealer")]
    public class DealerController : BaseApiController
    {
        private IDealerService _dealerService { get; set; }

        public DealerController(ILoggingService loggingService, IDealerService dealerService)
            : base(loggingService)
        {
            _dealerService = dealerService;
        }

        // GET: api/Contract
        [Route("GetDealerProfile")]
        [HttpGet]
        public IHttpActionResult GetDealerProfile()
        {
            try
            {            
                var dealerProfile = _dealerService.GetDealerProfile(LoggedInUser.UserId);
                return Ok(dealerProfile);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get profile for the Dealer {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }
        [Route("UpdateDealerProfile")]
        [HttpPost]
        public IHttpActionResult UpdateDealerProfile(DealerProfileDTO dealerProfile)
        {
            try
            {
                dealerProfile.DealerId = LoggedInUser.UserId;
                var result =  _dealerService.UpdateDealerProfile(dealerProfile);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("UpdateDealerOnboardingInfo")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult UpdateDealerOnboardingInfo(DealerInfoDTO dealerInfo)
        {
            try
            {
                var result = _dealerService.UpdateDealerOnboardingForm(dealerInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }            
        }

        [Route("SubmitDealerOnboardingInfo")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult SubmitDealerOnboardingInfo(DealerInfoDTO dealerInfo)
        {
            try
            {
                var result = _dealerService.SubmitDealerOnboardingForm(dealerInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("AddDocumentToDealerOnboarding")]
        [HttpPut]
        public IHttpActionResult AddDocumentToDealerOnboarding(RequiredDocumentDTO document)
        {
            try
            {
                var result = _dealerService.AddDocumentToOnboardingForm(document);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetDealerOnboardingInfo")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDealerOnboardingInfo(int dealerInfoId)
        {
            try
            {
                var dealerForm = _dealerService.GetDealerOnboardingForm(dealerInfoId);
                return Ok(dealerForm);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get dealer onboarding form with Id {dealerInfoId}", ex);
                return InternalServerError(ex);
            }
        }

        [Route("GetDealerOnboardingInfo")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDealerOnboardingInfo(string accessKey)
        {
            try
            {
                var dealerForm = _dealerService.GetDealerOnboardingForm(accessKey);
                return Ok(dealerForm);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get dealer onboarding form with access key {accessKey}", ex);
                return InternalServerError(ex);
            }
        }
    }
}
