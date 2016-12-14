using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Results;
using AutoMapper;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Integration.Utility;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Controllers
{
    using Models.Contract.EquipmentInformation;

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
            var contract = ContractService.GetContract(contractId, LoggedInUser?.UserId);
            if (contract != null)
            {
                return Ok(contract);
            }
            return NotFound();
        }

        [Route("GetContracts")]
        [HttpPost]
        public IHttpActionResult GetContracts(IEnumerable<int> ids)
        {
            try
            {
                var contracts = ContractService.GetContracts(ids, LoggedInUser?.UserId);
                return Ok(contracts);                
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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

        [Route("UpdateContractData")]
        [HttpPut]
        public IHttpActionResult UpdateContractData(ContractDataDTO contractData)
        {
            try
            {
                var alerts = ContractService.UpdateContractData(contractData, LoggedInUser?.UserId);
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
                var alerts = ContractService.InitiateCreditCheck(contractId, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("InitiateDigitalSignature")]
        [HttpPut]
        public IHttpActionResult InitiateDigitalSignature(SignatureUsersDTO users)
        {
            try
            {
                var alerts = ContractService.InitiateDigitalSignature(users.ContractId, LoggedInUser?.UserId, users.Users?.ToArray());
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("AddDocument")]
        [HttpPut]
        public IHttpActionResult AddDocumentToContract(ContractDocumentDTO document)
        {
            try
            {
                var result = ContractService.AddDocumentToContract(document, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("RemoveDocument")]
        [HttpPost]
        public IHttpActionResult RemoveContractDocument(int documentId)
        {
            try
            {
                var result = ContractService.RemoveContractDocument(documentId, LoggedInUser?.UserId);
                return Ok(result);
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
                var result = ContractService.GetCreditCheckResult(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetContractAgreement")]
        [HttpGet]
        public IHttpActionResult GetContractAgreement(int contractId)
        {
            try
            {
                var result = ContractService.GetPrintAgreement(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("CheckContractAgreementAvailable")]
        [HttpGet]
        public IHttpActionResult CheckContractAgreementAvailable(int contractId)
        {
            try
            {
                var result = ContractService.CheckPrintAgreementAvailable(contractId, LoggedInUser?.UserId);
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

        [Route("CreateXlsxReport")]
        [HttpPost]
        public HttpResponseMessage CreateXlsxReport(IEnumerable<int> ids)
        {
            try
            {
                var stream = new MemoryStream();
                var contracts = ContractService.GetContracts(ids, LoggedInUser?.UserId);
                XlsxExporter.Export(contracts, stream);

                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(stream.ToArray())
                };
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = $"{DateTime.Now.ToString(CultureInfo.CurrentCulture).Replace(":", ".")}-report.xlsx"
                    };
                result.Content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");
                return result;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        [Route("GetCustomer")]
        [HttpGet]
        public IHttpActionResult GetCustomer(int customerId)
        {
            var customer = ContractService.GetCustomer(customerId);
            if (customer != null)
            {
                return Ok(customer);
            }
            return NotFound();
        }

        [Route("UpdateCustomerData")]
        [HttpPut]
        public IHttpActionResult UpdateCustomerData([FromBody]CustomerDataDTO[] customers)
        {
            try
            {
                var alerts = ContractService.UpdateCustomers(customers, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("SubmitContract")]
        [HttpPost]
        public IHttpActionResult SubmitContract(int contractId)
        {
            try
            {
                var submitResult = ContractService.SubmitContract(contractId, LoggedInUser?.UserId);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("AddComment")]
        [HttpPost]
        public IHttpActionResult AddComment(CommentDTO comment)
        {
            try
            {
                var alerts = ContractService.AddComment(comment, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("RemoveComment")]
        [HttpPost]
        public IHttpActionResult RemoveComment(int commentId)
        {
            try
            {
                var alerts = ContractService.RemoveComment(commentId, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
