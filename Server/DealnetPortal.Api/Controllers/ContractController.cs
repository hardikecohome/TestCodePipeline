using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DealnetPortal.Api.Common.Constants;
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
            throw new NotImplementedException();
        }

        //Get: api/Contract/{contractId}
        [Route("{contractId}")]
        [HttpGet]
        public IHttpActionResult GetContract(int contractId)
        {
            throw new NotImplementedException();
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
                return BadRequest(ErrorConstants.ContractCreateFailed);
            }
        }

        [Route("UpdateContract")]
        [HttpPut]
        public IHttpActionResult UpdateContract(ContractDTO contractData)
        {
            throw new NotImplementedException();
        }

        [Route("UpdateContractClientData")]
        [HttpPut]
        public IHttpActionResult UpdateContractClientData(int contractId, ContractAddressDTO contractAddress, IList<CustomerDTO> customers)
        {
            throw new NotImplementedException();
        }

        [Route("InitiateCreditCheck")]
        [HttpPut]
        public IHttpActionResult InitiateCreditCheck(int contractId)
        {
            throw new NotImplementedException();
        }

        [Route("GetCreditCheckResult")]
        [HttpGet]
        public IHttpActionResult GetCreditCheckResult(int contractId)
        {
            throw new NotImplementedException();
        }

    }
}
