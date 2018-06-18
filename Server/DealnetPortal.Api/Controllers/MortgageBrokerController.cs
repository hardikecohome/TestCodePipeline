using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Utilities.Logging;
// ReSharper disable All

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/MortgageBroker")]
    public class MortgageBrokerController : BaseApiController
    {
        private IMortgageBrokerService _mortgageBrokerService { get; set; }
        private ICustomerWalletService _customerWalletService { get; set; }

        public MortgageBrokerController(ILoggingService loggingService, IMortgageBrokerService mortgageBrokerService,
            ICustomerWalletService customerWalletService) : base(loggingService)
        {
            _mortgageBrokerService = mortgageBrokerService;
            _customerWalletService = customerWalletService;
        }

        // GET: api/MortgageBroker
        [HttpGet]
        public IHttpActionResult GetCreatedContracts()
        {
            try
            {
                var contractsOffers = _mortgageBrokerService.GetCreatedContracts(LoggedInUser.UserId);
                return Ok(contractsOffers);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts created by User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        // POST: api/MortgageBroker
        [HttpPost]
        public async Task<IHttpActionResult> CreateContractForCustomer(NewCustomerDTO customerFormData)
        {
            var alerts = new List<Alert>();
            try
            {
                var creationResult = await _mortgageBrokerService.CreateContractForCustomer(LoggedInUser?.UserId, customerFormData);

                if (creationResult.Item1 == null)
                {
                    alerts.AddRange(creationResult.Item2);
                }
                return Ok(creationResult);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractCreateFailed,
                    Message = ex.ToString()
                });
                LoggingService.LogError(ErrorConstants.ContractCreateFailed, ex);
            }
            return Ok(new Tuple<ContractDTO, IList<Alert>>(null, alerts));
        }

        // GET: api/MortgageBroker/customers/{email}/check
        [Route("customers/{email}/check")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IHttpActionResult> CheckCustomerExistingAsync(string email)
        {
            try
            {
                var result = await _customerWalletService.CheckCustomerExisting(email);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}