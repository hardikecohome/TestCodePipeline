using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Web.Common;
using Microsoft.Owin.Security;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Web.ServiceAgent
{
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
			return await Client.PostAsyncEx<string, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}", "", AuthenticationHeader, CurrentCulture);
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

		public async Task<IList<ContractShortInfoDTO>> GetContractsShortInfo()
		{
			try
			{
				return await Client.GetAsyncEx<IList<ContractShortInfoDTO>>($"{_fullUri}/shortinfo", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get contracts for an user", ex);
				return new List<ContractShortInfoDTO>();
			}
		}

		public async Task<int> GetCustomersContractsCount()
		{
			try
			{            
				return await Client.GetAsyncEx<int>($"{_fullUri}/count", AuthenticationHeader, CurrentCulture);
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
				return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/completed", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get contracts for an user", ex);
				throw;
			}
		}

		public async Task<IList<ContractShortInfoDTO>> GetCompletedContractsShortInfo()
		{
			try
			{
				return await Client.GetAsyncEx<IList<ContractShortInfoDTO>>($"{_fullUri}/shortinfo/completed", AuthenticationHeader, CurrentCulture);
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
				return await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/leads", AuthenticationHeader, CurrentCulture);
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
				var idsUri = ids?.JoinStrings("&ids=");                
				var contracts = await Client.GetAsyncEx<IList<ContractDTO>>($"{_fullUri}/pack?ids={idsUri}", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/NotifyEdit", "", AuthenticationHeader, CurrentCulture);
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
					await Client.PutAsyncEx<ContractDataDTO, IList<Alert>>($"{_fullUri}", contractData, AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError($"Can't update client data for contract {contractData.Id}", ex);
				throw;
			}
		}

		public async Task<IList<Alert>> UpdateCustomerData(CustomerDataDTO[] customers)
		{
			try
			{
				return
					await Client.PutAsyncEx<CustomerDataDTO[], IList<Alert>>($"{_fullUri}/{customers?.FirstOrDefault()?.ContractId}/customers", customers, AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't update customers data", ex);
				throw;
			}            
		}		

		public async Task<Tuple<SignatureSummaryDTO, IList<Alert>>> InitiateDigitalSignature(SignatureUsersDTO signatureUsers)
		{
			try
			{
				return
					await Client.PutAsyncEx<SignatureUsersDTO, Tuple<SignatureSummaryDTO, IList<Alert>>>($"{_fullUri}/{signatureUsers.ContractId}/signature/initiate", signatureUsers, AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				this._loggingService.LogError($"Can't initiate digital signature for contract {signatureUsers.ContractId}", ex);
				throw;
			}
		}

		public async Task<IList<Alert>> UpdateContractSigners(SignatureUsersDTO signatureUsers)
		{
			try
			{
				return
					await Client.PutAsyncEx<SignatureUsersDTO, IList<Alert>>($"{_fullUri}/{signatureUsers.ContractId}/signature/signers", signatureUsers, AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError($"Can't update signature users for contract {signatureUsers.ContractId}", ex);
				throw;
			}
		}

		public async Task<Tuple<SignatureSummaryDTO, IList<Alert>>> CancelDigitalSignature(int contractId)
		{
			try
			{
				return
					await
						Client.PutAsyncEx<string, Tuple<SignatureSummaryDTO, IList<Alert>>>(
							$"{_fullUri}/{contractId}/signature/cancel", "", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError($"Can't cancel digital signature for contract {contractId}", ex);
				throw;
			}
		}        

		public async Task<Tuple<CreditCheckDTO, IList<Alert>>> SubmitContract(int contractId)
		{
			try
			{
				return
					await
						Client.PutAsyncEx<string, Tuple<CreditCheckDTO, IList<Alert>>>(
							$"{_fullUri}/{contractId}/Submit", "", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/creditcheck", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/documents/agreement", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get contract print agreement", ex);
				throw;
			}
		}

		public async Task<Tuple<IList<DocumentTypeDTO>, IList<Alert>>> GetContractDocumentTypes(int contractId)
		{
			try
			{
				return
					await
						Client.GetAsyncEx<Tuple<IList<DocumentTypeDTO>, IList<Alert>>>(
							$"{_fullUri}/{contractId}/DocumentTypes", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get document types list for contract", ex);
				throw;
			}
		}

		public async Task<Tuple<AgreementDocument, IList<Alert>>> GetSignedAgreement(int contractId)
		{
			try
			{
				return
					await
						Client.GetAsyncEx<Tuple<AgreementDocument, IList<Alert>>>(
							$"{_fullUri}/{contractId}/documents/agreement/Signed", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get contract document from Esignature", ex);
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
							$"{_fullUri}/{installationCertificateData.ContractId}/InstallationData", installationCertificateData, AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/documents/InstallationCertificate", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/documents/agreement/check", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{contractId}/documents/InstallationCertificate/check", AuthenticationHeader, CurrentCulture);
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
				IList<FlowingSummaryItemDTO> result = await Client.GetAsyncEx<IList<FlowingSummaryItemDTO>>($"{_fullUri}/Summary/{summaryType}", AuthenticationHeader, CurrentCulture);
				return result;
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't get credit contracts summary", ex);
				throw;
			}
		}       

		public async Task<AgreementDocument> GetXlsxReport(IEnumerable<int> ids, int? timeZoneOffset = null)
		{
			try
			{
                var uri = timeZoneOffset.HasValue
			        ? $"{_fullUri}/report/timezoneOffset={timeZoneOffset}"
			        : $"{_fullUri}/report";

                var report = await Client.PostAsyncEx< IEnumerable<int>, AgreementDocument> (uri, ids, AuthenticationHeader, CurrentCulture);
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

				return await Client.PostAsyncEx<ContractDocumentDTO, Tuple<int?, IList<Alert>>>($"{_fullUri}/{document.ContractId}/documents", document, 
					AuthenticationHeader, CurrentCulture, bsonFormatter);
				//return await result.Content.ReadAsAsync<Tuple<int?, IList<Alert>>>(formatters);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't add document to contract", ex);
				throw;
			}
		}

		public async Task<IList<Alert>> RemoveContractDocument(int contractId, int documentId)
		{
			try
			{
				return await Client.DeleteAsyncEx<IList<Alert>>(
							$"{_fullUri}/{contractId}/documents/{documentId}", AuthenticationHeader, CurrentCulture);
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
				return await Client.PutAsyncEx<string, IList<Alert>>(
							$"{_fullUri}/{contractId}/documents/submit", "", AuthenticationHeader, CurrentCulture);
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
							$"{_fullUri}/{comment.ContractId}/comments", comment, AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't add comment to contract", ex);
				throw;
			}
		}

		public async Task<IList<Alert>> RemoveComment(int contractId, int commentId)
		{
			try
			{
				return
					await
						Client.DeleteAsyncEx<IList<Alert>>(
							$"{_fullUri}/{contractId}/comments/{commentId}", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't remove comment to contract", ex);
				throw;
			}
		}

		public async Task<IList<Alert>> RemoveContract(int contractId)
		{
			try
			{
				return
					await
						Client.DeleteAsyncEx<IList<Alert>>(
							$"{_fullUri}/{contractId}", AuthenticationHeader, CurrentCulture);
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
						Client.PutAsyncEx<string, IList<Alert>>(
							$"{_fullUri}/{contractId}/Assign", "", AuthenticationHeader, CurrentCulture);
			}
			catch (Exception ex)
			{
				_loggingService.LogError("Can't assign contract", ex);
				throw;
			}
		}

		
	}
}
