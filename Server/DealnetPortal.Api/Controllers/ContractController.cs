using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Interfaces;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Contract")]
    public class ContractController : BaseApiController
    {
        private IContractService _contractService { get; set; }
        private IDocumentService _documentService { get; set; }
        private ICreditCheckService _creditCheckService { get; set; }

        public ContractController(ILoggingService loggingService, IContractService contractService,
            IDocumentService documentService, ICreditCheckService creditCheckService)
            : base(loggingService)
        {
            _contractService = contractService;
            _documentService = documentService;
            _creditCheckService = creditCheckService;
        }

        // GET: api/Contract
        [HttpGet]
        public IHttpActionResult GetContract()
        {
            try
            {            
                var contracts = _contractService.GetContracts<ContractDTO>(LoggedInUser.UserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        // GET: api/Contract/shortinfo
        [HttpGet]
        [Route("shortinfo")]
        public IHttpActionResult GetContractsShortInfo()
        {
            try
            {
                var contracts = _contractService.GetContracts<ContractShortInfoDTO>(LoggedInUser.UserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        // GET: api/Contract/count
        [Route("count")]
        [HttpGet]
        public IHttpActionResult GetCustomersContractsCount()
        {
            try
            {
                var contracts = _contractService.GetCustomersContractsCount(LoggedInUser.UserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get number of customers contracts for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        // GET: api/Contract/completed
        [Route("completed")]
        [HttpGet]
        public IHttpActionResult GetCompletedContracts()
        {
            var contracts = _contractService.GetContracts<ContractDTO>(c => c.ContractState >= ContractState.Completed, LoggedInUser.UserId);
            return Ok(contracts);
        }

        [Route("shortinfo/completed")]
        [HttpGet]
        public IHttpActionResult GetCompletedContractsShortInfo()
        {
            var contracts = _contractService.GetContracts<ContractShortInfoDTO>(c => c.ContractState >= ContractState.Completed, LoggedInUser.UserId);
            return Ok(contracts);
        }

        //Get: api/Contract/{contractId}
        [Route("{contractId}")]
        [HttpGet]
        public IHttpActionResult GetContract(int contractId)
        {
            var contract = _contractService.GetContract(contractId, LoggedInUser?.UserId);
            if (contract != null)
            {
                return Ok(contract);
            }

            return NotFound();
        }

        [Route("pack")]
        [HttpGet]
        public IHttpActionResult GetContracts([FromUri] IEnumerable<int> ids)
        {
            try
            {
                var contracts = _contractService.GetContracts(ids, LoggedInUser?.UserId);
                return Ok(contracts);                
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/Contract/leads
        [Route("leads")]
        [HttpGet]
        public IHttpActionResult GetDealerLeads()
        {
            try
            {
                var contractsOffers = _contractService.GetDealerLeads(LoggedInUser.UserId);
                return Ok(contractsOffers);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts offers for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        // POST: api/Contract
        [HttpPost]
        public IHttpActionResult CreateContract()
        {
            var alerts = new List<Alert>();
            try
            {
                var contract = _contractService.CreateContract(LoggedInUser?.UserId);
                if (contract == null)
                {
                    alerts.Add(new Alert
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
                alerts.Add(new Alert
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.ContractCreateFailed,
                    Message = ex.ToString()
                });
            }
            return Ok(new Tuple<ContractDTO, IList<Alert>>(null, alerts));
        }

        // PUT: api/Contract
        [HttpPut]
        public IHttpActionResult UpdateContractData(ContractDataDTO contractData)
        {
            try
            {
                var alerts = _contractService.UpdateContractData(contractData, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/NotifyEdit")]
        [HttpPut]
        public IHttpActionResult NotifyContractEdit(int contractId)
        {
            try
            {
                var alerts = _contractService.NotifyContractEdit(contractId, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/signature/initiate")]
        [HttpPut]
        public async Task<IHttpActionResult> InitiateDigitalSignature(int contractId, SignatureUsersDTO users)
        {
            try
            {
                var summary = await _documentService.StartSignatureProcess(users.ContractId, LoggedInUser?.UserId, users.Users?.ToArray());
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/signature/signers")]
        [HttpPut]
        public async Task<IHttpActionResult> UpdateContractSigners(int contractId, SignatureUsersDTO users)
        {
            try
            {
                var alerts = await _documentService.UpdateSignatureUsers(users.ContractId, LoggedInUser?.UserId, users.Users?.ToArray());
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/signature/cancel")]
        [HttpPut]
        public async Task<IHttpActionResult> CancelDigitalSignature(int contractId)
        {
            try
            {
                var result = await _documentService.CancelSignatureProcess(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }        

        [Route("{contractId}/documents")]
        [HttpPost]
        public IHttpActionResult AddDocumentToContract(int contractId, ContractDocumentDTO document)
        {
            try
            {
                var result = _contractService.AddDocumentToContract(document, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/{documentId}")]
        [HttpDelete]
        public IHttpActionResult RemoveContractDocument(int contractId, int documentId)
        {
            try
            {
                var result = _contractService.RemoveContractDocument(documentId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/submit")]
        [HttpPut]
        public async Task<IHttpActionResult> SubmitAllDocumentsUploaded(int contractId)
        {
            try
            {
                var result = await _contractService.SubmitAllDocumentsUploaded(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/creditcheck")]
        [HttpGet]
        public IHttpActionResult GetCreditCheckResult(int contractId)
        {
            try
            {
                var result = _creditCheckService.ContractCreditCheck(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/DocumentTypes")]
        [HttpGet]
        public IHttpActionResult GetContractDocumentTypes(int contractId)
        {
            try
            {
                var result = _contractService.GetContractDocumentTypes(contractId, LoggedInUser?.UserId);                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/agreement")]
        [HttpGet]
        public async Task<IHttpActionResult> GetContractAgreement(int contractId)
        {
            try
            {
                var result = await _documentService.GetPrintAgreement(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/agreement/signed")]
        [HttpGet]
        public async Task<IHttpActionResult> GetSignedAgreement(int contractId)
        {
            try
            {                
                var result = await _documentService.GetSignedAgreement(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/agreement/check")]
        [HttpGet]
        public async Task<IHttpActionResult> CheckContractAgreementAvailable(int contractId)
        {
            try
            {
                var result = await _documentService.CheckPrintAgreementAvailable(contractId, (int) DocumentTemplateType.SignedContract, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/InstallationCertificate")]
        [HttpGet]
        public async Task<IHttpActionResult> GetInstallationCertificate(int contractId)
        {
            try
            {                
                var result = await _documentService.GetInstallCertificate(contractId, LoggedInUser?.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/documents/InstallationCertificate/check")]
        [HttpGet]
        public async Task<IHttpActionResult> CheckInstallationCertificateAvailable(int contractId)
        {
            try
            {
                var result = await _documentService.CheckPrintAgreementAvailable(contractId, (int)DocumentTemplateType.SignedInstallationCertificate, LoggedInUser?.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("Summary/{summaryType}")]
        [HttpGet]
        public IHttpActionResult GetContractsSummary(string summaryType)
        {
            FlowingSummaryType type;
            Enum.TryParse(summaryType, out type);

            var result = _contractService.GetDealsFlowingSummary(LoggedInUser?.UserId, type);
            return Ok(result);
        }        

        [Route("pack/report")]
        [Route("pack/report/timezoneOffset={timezoneOffset}")]
        [HttpGet]
        public IHttpActionResult GetXlsxReport([FromUri] IEnumerable<int> ids, int? timezoneOffset = null)
        {
            try
            {
                var report = _contractService.GetContractsFileReport(ids, LoggedInUser.UserId, timezoneOffset);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }        

        
        [Route("{contractId}/customers")]
        [HttpPut]
        public IHttpActionResult UpdateCustomerData(int contractId, [FromBody]CustomerDataDTO[] customers)
        {
            try
            {
                var alerts = _contractService.UpdateCustomers(customers, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/InstallationData")]
        [HttpPut]
        public IHttpActionResult UpdateInstallationData(int contractId, InstallationCertificateDataDTO installationCertificateData)
        {
            try
            {
                var alerts = _contractService.UpdateInstallationData(installationCertificateData, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/Submit")]
        [HttpPut]
        public IHttpActionResult SubmitContract(int contractId)
        {
            try
            {
                var submitResult = _contractService.SubmitContract(contractId, LoggedInUser?.UserId);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/comments")]
        [HttpPost]
        public IHttpActionResult AddComment(int contractId, CommentDTO comment)
        {
            try
            {
                var alerts = _contractService.AddComment(comment, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/comments/{commentId}")]
        [HttpDelete]
        public IHttpActionResult RemoveComment(int contractId, int commentId)
        {
            try
            {
                var alerts = _contractService.RemoveComment(commentId, LoggedInUser?.UserId);
                return Ok(alerts);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }        

        [Route("{contractId}")]
        [HttpDelete]
        public IHttpActionResult RemoveContract(int contractId)
        {
            try
            {
                var result = _contractService.RemoveContract(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("{contractId}/Assign")]
        [HttpPut]
        public async Task<IHttpActionResult> AssignContract(int contractId)
        {
            try
            {
                var result = await _contractService.AssignContract(contractId, LoggedInUser?.UserId).ConfigureAwait(false);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }        
    }
}
