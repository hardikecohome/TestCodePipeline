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
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Integration.Utility;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Controllers
{
    [Authorize]
    [RoutePrefix("api/Contract")]
    public class ContractController : BaseApiController
    {
        private IContractService ContractService { get; set; }
        private ICustomerFormService CustomerFormService { get; set; }
        private IRateCardsService RateCardsService { get; set; }

        public ContractController(ILoggingService loggingService, IContractService contractService, ICustomerFormService customerFormService, IRateCardsService rateCardsService)
            : base(loggingService)
        {
            ContractService = contractService;
            CustomerFormService = customerFormService;
            RateCardsService = rateCardsService;
        }

        // GET: api/Contract
        [HttpGet]
        public IHttpActionResult GetContract()
        {
            try
            {            
                var contracts = ContractService.GetContracts(LoggedInUser.UserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        [Route("GetCustomersContractsCount")]
        [HttpGet]
        public IHttpActionResult GetCustomersContractsCount()
        {
            try
            {
                var contracts = ContractService.GetCustomersContractsCount(LoggedInUser.UserId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get number of customers contracts for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        [Route("GetCompletedContracts")]
        [HttpGet]
        public IHttpActionResult GetCompletedContracts()
        {
            var contracts = ContractService.GetContracts(LoggedInUser.UserId);
            return Ok(contracts.Where(c => c.ContractState >= ContractState.Completed));
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

        [Route("GetDealerLeads")]
        [HttpGet]
        public IHttpActionResult GetDealerLeads()
        {
            try
            {
                var contractsOffers = ContractService.GetDealerLeads(LoggedInUser.UserId);
                return Ok(contractsOffers);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts offers for the User {LoggedInUser.UserId}", ex);
                return InternalServerError(ex);
            }
        }

        [Route("GetCreatedContracts")]
        [HttpGet]
        public IHttpActionResult GetCreatedContracts()
        {
            try
            {
                var contractsOffers = ContractService.GetCreatedContracts(LoggedInUser.UserId);
                return Ok(contractsOffers);
            }
            catch (Exception ex)
            {
                LoggingService.LogError($"Failed to get contracts created by User {LoggedInUser.UserId}", ex);
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

        [Route("NotifyContractEdit")]
        [HttpPut]
        public IHttpActionResult NotifyContractEdit(int contractId)
        {
            try
            {
                var alerts = ContractService.NotifyContractEdit(contractId, LoggedInUser?.UserId);
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

        [Route("SubmitAllDocumentsUploaded")]
        [HttpPost]
        public async Task<IHttpActionResult> SubmitAllDocumentsUploaded(int contractId)
        {
            try
            {
                var result = await ContractService.SubmitAllDocumentsUploaded(contractId, LoggedInUser?.UserId);
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
                var result = ContractService.CheckPrintAgreementAvailable(contractId, (int) DocumentTemplateType.SignedContract, LoggedInUser?.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetInstallationCertificate")]
        [HttpGet]
        public IHttpActionResult GetInstallationCertificate(int contractId)
        {
            try
            {                
                var result = ContractService.GetInstallCertificate(contractId, LoggedInUser?.UserId);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("CheckInstallationCertificateAvailable")]
        [HttpGet]
        public IHttpActionResult CheckInstallationCertificateAvailable(int contractId)
        {
            try
            {
                var result = ContractService.CheckPrintAgreementAvailable(contractId, (int)DocumentTemplateType.SignedInstallationCertificate, LoggedInUser?.UserId);

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
        public IHttpActionResult CreateXlsxReport(IEnumerable<int> ids)
        {
            try
            {
                var report = ContractService.GetContractsFileReport(ids, LoggedInUser.UserId);
                return Ok(report);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
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

        [Route("UpdateInstallationData")]
        [HttpPut]
        public IHttpActionResult UpdateInstallationData(InstallationCertificateDataDTO installationCertificateData)
        {
            try
            {
                var alerts = ContractService.UpdateInstallationData(installationCertificateData, LoggedInUser?.UserId);
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

        [Route("SubmitCustomerForm")]
        [HttpPost]
        [AllowAnonymous]
        public IHttpActionResult SubmitCustomerForm(CustomerFormDTO customerFormData)
        {
            try
            {
                var submitResult = CustomerFormService.SubmitCustomerFormData(customerFormData);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("SubmitCustomerServiceRequest")]
        [HttpPost]
        [AllowAnonymous]
        public async Task<IHttpActionResult> SubmitCustomerServiceRequest(CustomerServiceRequestDTO customerServiceRequest)
        {
            try
            {
                var submitResult = await CustomerFormService.CustomerServiceRequest(customerServiceRequest).ConfigureAwait(false);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetCustomerContractInfo")]
        [HttpGet]
        // GET api//Contract/GetCustomerContractInfo?contractId={contractId}&dealerName={dealerName}
        [AllowAnonymous]
        public IHttpActionResult GetCustomerContractInfo(int contractId, string dealerName)
        {
            try
            {
                var submitResult = CustomerFormService.GetCustomerContractInfo(contractId, dealerName);
                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("CreateContractForCustomer")]
        [HttpPost]
        public async Task<IHttpActionResult> CreateContractForCustomer(NewCustomerDTO customerFormData)
        {
            var alerts = new List<Alert>();
            try
            {
                var creationResult = await ContractService.CreateContractForCustomer(LoggedInUser?.UserId, customerFormData);

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
            }

            return Ok(new Tuple<ContractDTO, IList<Alert>>(null, alerts));
        }                

        [Route("RemoveContract")]
        [HttpPost]
        public IHttpActionResult RemoveContract(int contractId)
        {
            try
            {
                var result = ContractService.RemoveContract(contractId, LoggedInUser?.UserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("AssignContract")]
        [HttpPost]
        public async Task<IHttpActionResult> AssignContract(int contractId)
        {
            try
            {
                var result = await ContractService.AssignContract(contractId, LoggedInUser?.UserId).ConfigureAwait(false);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [Route("GetDealerTier")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult GetDealerTier()
        {
            try
            {
                var submitResult = RateCardsService.GetRateCardsByDealerId(LoggedInUser?.UserId);

                return Ok(submitResult);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
