using System;
using System.Threading.Tasks;
using System.Web.Http;

using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.DealerOnboarding;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Api.Models.Notify;

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

        /// dealer profile endpoints
                
        [Route("Profile")]
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

        [Route("Profile")]
        [HttpPut]
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

        [Route("DealerSupportRequestEmail")]
        [HttpPost]
        public IHttpActionResult DealerSupportRequestEmail(SupportRequestDTO dealerSupportRequest)
        {
            try
            {
                //dealerProfile.DealerId = LoggedInUser.UserId;
                var result = _dealerService.DealerSupportRequestEmail(dealerSupportRequest);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // dealer onboarding endpoints
        [Route("onboarding/checkLink/{dealerLink}")]
        [Route("CheckOnboardingLink")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult CheckOnboardingLink(string dealerLink)
        {
            try
            {
                var result = _dealerService.CheckOnboardingLink(dealerLink);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("onboarding")]
        [Route("UpdateDealerOnboardingInfo")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> UpdateDealerOnboardingInfo(DealerInfoDTO dealerInfo)
        {
            try
            {
                var result = await _dealerService.UpdateDealerOnboardingForm(dealerInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }            
        }

        [Route("onboarding/submit")]
        [Route("SubmitDealerOnboardingInfo")]
        [HttpPut]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SubmitDealerOnboardingInfo(DealerInfoDTO dealerInfo)
        {
            try
            {
                var result = await _dealerService.SubmitDealerOnboardingForm(dealerInfo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetDealerOnboardingInfo")]
        [Route("onboarding/{dealerInfoId}")]
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

        [Route("onboarding/key/{accessKey}")]
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

        [Route("onboarding/sendLink")]
        [Route("SendDealerOnboardingDraftLink")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SendDealerOnboardingDraftLink(DraftLinkDTO link)
        {
            try
            {
                var result = await _dealerService.SendDealerOnboardingDraftLink(link);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Onboarding/{dealerInfoId}/documents")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> AddDocumentToDealerOnboarding(int dealerInfoId, RequiredDocumentDTO document)
        {
            try
            {
                var result = await _dealerService.AddDocumentToOnboardingForm(document);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Onboarding/{dealerInfoId}/documents/{documentId}")]
        [HttpDelete]
        [AllowAnonymous]
        public IHttpActionResult DeleteDocumentFromOnboardingForm(int dealerInfoId, int documentId)
        {
            try
            {
                var document = new RequiredDocumentDTO()
                {
                    DealerInfoId = dealerInfoId,
                    Id = documentId
                };
                var result = _dealerService.DeleteDocumentFromOnboardingForm(document);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        
        
    }
}
