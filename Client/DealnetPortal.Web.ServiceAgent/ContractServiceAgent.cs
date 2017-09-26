using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;
using Microsoft.Owin.Security;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;

    public class ContractServiceAgent : ApiBase, IContractServiceAgent
    {
        private const string ContractApi = "Contract";
        private readonly ILoggingService _loggingService;

        public ContractServiceAgent(IHttpApiClient client, ILoggingService loggingService, IAuthenticationManager authenticationManager)
            : base(client, ContractApi, authenticationManager)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContract()
        {
            return await Client.PutAsyncEx<string, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}/CreateContract", "", AuthenticationHeader, CurrentCulture);
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> GetContract(int contractId)
        {
            var alerts = new List<Alert>();
            try
            {
                var contract = await Client.GetAsyncEx<ContractDTO>($"{_fullUri}/{contractId}", AuthenticationHeader, CurrentCulture);
                return new Tuple<ContractDTO, IList<Alert>>(contract, alerts);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Can't get contract with id {contractId}",
                    Message = ex.Message
                });
                _loggingService.LogError($"Can't get contract with id {contractId}", ex);
            }
            return new Tuple<ContractDTO, IList<Alert>>(null, alerts);
        }

        public async Task<IList<ContractDTO>> GetContracts()
        {
            try
            {                
                return await Client.GetAsyncEx<IList<ContractDTO>>(_fullUri, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts for an user", ex);
                return new List<ContractDTO>();
                //throw;
            }
        }

        public async Task<IList<ContractDTO>> GetCreatedContracts()
        {
            try
            {
                return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/GetCreatedContracts", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts created by an user", ex);
                return new List<ContractDTO>();
            }
        }

        public async Task<int> GetCustomersContractsCount()
        {
            try
            {            
                return await Client.GetAsyncEx<int>($"{_fullUri}/GetCustomersContractsCount", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get number of customer contracts for an user", ex);
                return 0;
            }
        }

        public async Task<IList<ContractDTO>> GetCompletedContracts()
        {
            try
            {
                return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/GetCompletedContracts", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts for an user", ex);
                throw;
            }
        }

        public async Task<IList<ContractDTO>> GetLeads()
        {
            try
            {
                return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/GetDealerLeads", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contracts for an user", ex);
                throw;
            }
        }

        public async Task<Tuple<IList<ContractDTO>, IList<Alert>>> GetContracts(IEnumerable<int> ids)
        {
            var alerts = new List<Alert>();
            try
            {
                var contracts = await Client.PostAsyncEx<IEnumerable<int>, IList<ContractDTO>>($"{_fullUri}/GetContracts", ids, AuthenticationHeader, CurrentCulture);
                return new Tuple<IList<ContractDTO>, IList<Alert>>(contracts, alerts);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Can't get contracts",
                    Message = ex.Message
                });
                _loggingService.LogError($"Can't get contracts", ex);
            }
            return new Tuple<IList<ContractDTO>, IList<Alert>>(null, alerts);
        }

        public async Task<IList<Alert>> NotifyContractEdit(int contractId)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/NotifyContractEdit?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't update contract {contractId} data", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateContractData(ContractDataDTO contractData)
        {
            try
            {
                return
                    await Client.PutAsyncEx<ContractDataDTO, IList<Alert>>($"{_fullUri}/UpdateContractData", contractData, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError($"Can't update client data for contract {contractData.Id}", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateCustomerData(CustomerDataDTO[] customers)
        {
            try
            {
                return
                    await Client.PutAsyncEx<CustomerDataDTO[], IList<Alert>>($"{_fullUri}/UpdateCustomerData", customers, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError("Can't update customers data", ex);
                throw;
            }            
        }

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/InitiateCreditCheck?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't initiate credit check for contract {contractId}", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> InitiateDigitalSignature(SignatureUsersDTO signatureUsers)
        {
            try
            {
                return
                    await Client.PutAsyncEx<SignatureUsersDTO, IList<Alert>>($"{_fullUri}/InitiateDigitalSignature", signatureUsers, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError($"Can't initiate digital signature for contract {signatureUsers.ContractId}", ex);
                throw;
            }
        }

        public async Task<Tuple<CreditCheckDTO, IList<Alert>>> SubmitContract(int contractId)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<string, Tuple<CreditCheckDTO, IList<Alert>>>(
                            $"{_fullUri}/SubmitContract?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit contract", ex);
                throw;
            }
        }

        public async Task<Tuple<CreditCheckDTO, IList<Alert>>> GetCreditCheckResult(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<Tuple<CreditCheckDTO, IList<Alert>>>(
                            $"{_fullUri}/GetCreditCheckResult?contractId={contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't get credit check result for contract {contractId}", ex);
                throw;
            }
        }

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<Tuple<AgreementDocument, IList<Alert>>>(
                            $"{_fullUri}/GetContractAgreement?contractId={contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contract print agreement", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> UpdateInstallationData(InstallationCertificateDataDTO installationCertificateData)
        {
            try
            {
                return
                    await
                        Client.PutAsyncEx<InstallationCertificateDataDTO, IList<Alert>>(
                            $"{_fullUri}/UpdateInstallationData", installationCertificateData, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't update installation certificate data", ex);
                throw;
            }
        }

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetInstallationCertificate(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<Tuple<AgreementDocument, IList<Alert>>>(
                            $"{_fullUri}/GetInstallationCertificate?contractId={contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get installation certificate", ex);
                throw;
            }
        }

        public async Task<Tuple<bool, IList<Alert>>> CheckContractAgreementAvailable(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<Tuple<bool, IList<Alert>>>(
                            $"{_fullUri}/CheckContractAgreementAvailable?contractId={contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't check contract print agreement", ex);
                throw;
            }
        }

        public async Task<Tuple<bool, IList<Alert>>> CheckInstallationCertificateAvailable(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<Tuple<bool, IList<Alert>>>(
                            $"{_fullUri}/CheckInstallationCertificateAvailable?contractId={contractId}", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't check installation certificate", ex);
                throw;
            }
        }

        public async Task<IList<FlowingSummaryItemDTO>> GetContractsSummary(string summaryType)
        {
            try
            {            
                IList<FlowingSummaryItemDTO> result = await Client.GetAsyncEx<IList<FlowingSummaryItemDTO>>($"{_fullUri}/{summaryType}/ContractsSummary", AuthenticationHeader, CurrentCulture);
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get credit contracts summary", ex);
                throw;
            }
        }       

        public async Task<AgreementDocument> GetXlsxReport(IEnumerable<int> ids)
        {
            try
            {
                var report = await Client.PostAsyncEx<IEnumerable<int>, AgreementDocument> ($"{_fullUri}/CreateXlsxReport", ids, AuthenticationHeader, CurrentCulture);
                return report;
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't get xlsx report", ex);
                throw;
            }
        }

        public async Task<Tuple<int?, IList<Alert>>> AddDocumentToContract(ContractDocumentDTO document)
        {
            try
            {
                MediaTypeFormatter bsonFormatter = new BsonMediaTypeFormatter();
                MediaTypeFormatter[] formatters = new MediaTypeFormatter[] { bsonFormatter, };

                return await Client.PutAsyncEx<ContractDocumentDTO, Tuple<int?, IList<Alert>>>($"{_fullUri}/AddDocument", document, 
                    AuthenticationHeader, CurrentCulture, bsonFormatter);
                //return await result.Content.ReadAsAsync<Tuple<int?, IList<Alert>>>(formatters);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError("Can't add document to contract", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> RemoveContractDocument(int documentId)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/RemoveDocument?documentId={documentId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove contract document", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> SubmitAllDocumentsUploaded(int contractId)
        {
            var alerts = new List<Alert>();
            try
            {
                return await Client.PostAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/SubmitAllDocumentsUploaded?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = $"Can't submit All Documents UploadedRequest",
                    Message = ex.Message
                });
                _loggingService.LogError($"Can't submit All Documents UploadedRequest", ex);
            }
            return new List<Alert>(alerts);
        }

        public async Task<Tuple<int?, IList<Alert>>> AddComment(CommentDTO comment)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<CommentDTO, Tuple<int?, IList<Alert>>>(
                            $"{_fullUri}/AddComment", comment, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't add comment to contract", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> RemoveComment(int commentId)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/RemoveComment?commentId={commentId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove comment to contract", ex);
                throw;
            }
        }

        public async Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormDTO customerForm)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<CustomerFormDTO, Tuple<CustomerContractInfoDTO, IList<Alert>>>(
                            $"{_fullUri}/SubmitCustomerForm", customerForm, null, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit Customer Form", ex);
                throw;
            }
        }

        public async Task<CustomerContractInfoDTO> GetCustomerContractInfo(int contractId, string dealerName)
        {
            try
            {                
                return await Client.GetAsyncEx<CustomerContractInfoDTO>(
                        $"{_fullUri}/GetCustomerContractInfo?contractId={contractId}&dealerName={dealerName}", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);            
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit Customer contract info", ex);
                throw;
            }
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContractForCustomer(NewCustomerDTO customerForm)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<NewCustomerDTO, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}/CreateContractForCustomer", customerForm, AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't submit New Customer", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> CheckCustomerExisting(string email)
        {
            try
            {
                return
                    await
                        Client.GetAsyncEx<IList<Alert>>($"{_fullUri}/CheckCustomerExisting?email={email}", AuthenticationHeader, CurrentCulture).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove contract", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> RemoveContract(int contractId)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/RemoveContract?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove contract", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> AssignContract(int contractId)
        {
            try
            {
                return
                    await
                        Client.PostAsyncEx<string, IList<Alert>>(
                            $"{_fullUri}/AssignContract?contractId={contractId}", "", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't assign contract", ex);
                throw;
            }
        }

        public async Task<TierDTO> GetDealerTier()
        {
            try
            {
                return await Client.GetAsyncEx<TierDTO>($"{_fullUri}/GetDealerTier", AuthenticationHeader, CurrentCulture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove contract", ex);
                throw;
            }
        }
    }
}
