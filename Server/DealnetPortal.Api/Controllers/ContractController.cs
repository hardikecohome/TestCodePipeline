using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    public class ContractController : BaseApiController
    {
        private IContractService ContractService { get; set; }

        public ContractController(ILoggingService loggingService, IContractService contractService)
            : base(loggingService)
        {
            ContractService = contractService;
        }

        // GET: api/Contract
        [HttpGet]
        public IHttpActionResult GetContract()
        {
            var contracts = ContractService.GetContracts(LoggedInUser.UserId);
            return Ok(contracts);
        }

        //Get: api/Contract/{contractId}
        [Route("{contractId}")]
        [HttpGet]
        public IHttpActionResult GetContract(int contractId)
        {
            var contract = ContractService.GetContract(contractId);
            if (contract != null)
            {
                return Ok(contract);
            }
            return NotFound();
        }

        [Route("CreateContract")]
        [HttpPut]
        public IHttpActionResult CreateContract()
        {
            try
            {
                var contract = ContractService.CreateContract(LoggedInUser?.UserId);
                if (contract != null)
                {
                    return Ok(contract);
                }
                else
                {
                    return BadRequest(ErrorConstants.ContractCreateFailed);
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        //[Route("UpdateContract")]
        //[HttpPut]
        //public IHttpActionResult UpdateContract(ContractDTO contractData)
        //{
        //    throw new NotImplementedException();
        //}

        [Route("UpdateContractClientData")]
        [HttpPut]
        public IHttpActionResult UpdateContractClientData(int contractId, IList<ContractAddressDTO> addresses, IList<CustomerDTO> customers)
        {
            try
            {
                var alerts = ContractService.UpdateContractClientData(contractId, addresses, customers);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("InitiateCreditCheck")]
        [HttpPut]
        public IHttpActionResult InitiateCreditCheck(int contractId)
        {
            try
            {
                var alerts = ContractService.InitiateCreditCheck(contractId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetCreditCheckResult")]
        [HttpGet]
        public IHttpActionResult GetCreditCheckResult(int contractId)
        {
            try
            {
                var result = ContractService.GetCreditCheckResult(contractId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

    }
}
