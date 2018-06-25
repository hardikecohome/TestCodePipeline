using System;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/CustomerForm")]
    public class CustomerFormController : BaseApiController
    {
        private ICustomerFormService _customerFormService { get; set; }

        public CustomerFormController(ILoggingService loggingService, ICustomerFormService customerFormService) 
            : base(loggingService)
        {
            _customerFormService = customerFormService;
        }
        
        [HttpPost]
        public IHttpActionResult SubmitCustomerForm(CustomerFormDTO customerFormData)
        {
            try
            {
                var submitResult = _customerFormService.SubmitCustomerFormData(customerFormData);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/{dealerName}")]
        [Route("{contractId}")]
        [HttpGet]
        public IHttpActionResult GetCustomerContractInfo(int contractId, string dealerName)
        {
            try
            {
                var submitResult = _customerFormService.GetCustomerContractInfo(contractId, dealerName);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Authorize]
        [HttpGet]
        // GET api/CustomerForm/Settings
        [Route("Settings")]
        public IHttpActionResult GetCustomerLinkSettings()
        {
            var linkSettings = _customerFormService.GetCustomerLinkSettings(LoggedInUser?.UserId);
            if (linkSettings != null)
            {
                return Ok(linkSettings);
            }
            return NotFound();
        }

        [HttpGet]
        // GET api/CustomerForm/Settings/{dealer}
        [Route("Settings/{dealer}")]
        public IHttpActionResult GetCustomerLinkSettings(string dealer)
        {
            var linkSettings = _customerFormService.GetCustomerLinkSettingsByDealerName(dealer);
            if (linkSettings != null)
            {
                return Ok(linkSettings);
            }
            return NotFound();
        }

        [Authorize]
        [HttpPut]
        // GET api/CustomerForm/Settings
        [Route("Settings")]
        public IHttpActionResult UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings)
        {
            try
            {
                var alerts = _customerFormService.UpdateCustomerLinkSettings(customerLinkSettings, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        // GET api/CustomerForm/LinkOptions/{hashDealerName}/{lang}
        [Route("LinkOptions/{hashDealerName}/{lang}")]
        public IHttpActionResult GetCustomerLinkLanguageOptions(string hashDealerName, string lang)
        {
            var linkSettings = _customerFormService.GetCustomerLinkLanguageOptions(hashDealerName, lang);
            if (linkSettings != null)
            {
                return Ok(linkSettings);
            }
            return NotFound();
        }


        //POST: api/CustomerForm/ServiceRequest
        [Route("ServiceRequest")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SubmitCustomerServiceRequest(CustomerServiceRequestDTO customerServiceRequest)
        {
            try
            {
                var submitResult = await _customerFormService.CustomerServiceRequest(customerServiceRequest).ConfigureAwait(false);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}