﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Contract")]
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
            var alerts = new List<Alert>();
            try
            {
                var contract = ContractService.CreateContract(LoggedInUser?.UserId);
                if (contract == null)
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = ErrorConstants.ContractCreateFailed,
                        Message = $"Failed to create contract for a user [{LoggedInUser?.UserId}]"
                    });
                }
                return Ok(new Tuple<ContractDTO, IList<Alert>>(contract, alerts));
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractCreateFailed,
                    Message = ex.ToString()
                });
            }
            return Ok(new Tuple<ContractDTO, IList<Alert>>(null, alerts));
        }

        //[Route("UpdateContract")]
        //[HttpPut]
        //public IHttpActionResult UpdateContract(ContractDTO contractData)
        //{
        //    throw new NotImplementedException();
        //}

        [Route("UpdateContractData")]
        [HttpPut]
        public IHttpActionResult UpdateContractData(ContractDataDTO contractData)
        {
            try
            {
                var alerts = ContractService.UpdateContractData(contractData);
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

        [Route("{summaryType}/ContractsSummary")]
        [HttpGet]
        public IHttpActionResult GetContractsSummary(string summaryType)
        {
            FlowingSummaryType type;
            Enum.TryParse(summaryType, out type);

            var result = ContractService.GetDealsFlowingSummary(LoggedInUser?.UserId, type);
            return Ok(result);
        }
    }
}
