﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Utilities;

namespace DealnetPortal.Web.ServiceAgent
{
    using Api.Models.Contract.EquipmentInformation;

    public class ContractServiceAgent : ApiBase, IContractServiceAgent
    {
        private const string ContractApi = "Contract";
        private readonly ILoggingService _loggingService;

        public ContractServiceAgent(IHttpApiClient client, ILoggingService loggingService)
            : base(client, ContractApi)
        {
            _loggingService = loggingService;
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> CreateContract()
        {
            return await Client.PutAsync<string, Tuple<ContractDTO, IList<Alert>>>($"{_fullUri}/CreateContract", "");
        }

        public async Task<Tuple<ContractDTO, IList<Alert>>> GetContract(int contractId)
        {
            var alerts = new List<Alert>();
            try
            {
                var contract = await Client.GetAsync<ContractDTO>($"{_fullUri}/{contractId}");
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
                return await Client.GetAsync<IList<ContractDTO>>(_fullUri);
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
                var contracts = await Client.PostAsync<IEnumerable<int>, IList<ContractDTO>>($"{_fullUri}/GetContracts", ids);
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

        public async Task<IList<Alert>> UpdateContractData(ContractDataDTO contractData)
        {
            try
            {
                return
                    await Client.PutAsync<ContractDataDTO, IList<Alert>>($"{_fullUri}/UpdateContractData", contractData);
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
                    await Client.PutAsync<CustomerDataDTO[], IList<Alert>>($"{_fullUri}/UpdateCustomerData", customers);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError("Can't update customers data", ex);
                throw;
            }            
        }

        //public async Task<IList<Alert>> UpdateEquipmentInformation(EquipmentInfoDTO equipmentInfo)
        //{
        //    try
        //    {
        //        return
        //            await this.Client.PutAsync<EquipmentInfoDTO, IList<Alert>>($"{_fullUri}/UpdateEquipmentInformation", equipmentInfo);
        //    }
        //    catch (Exception ex)
        //    {
        //        this._loggingService.LogError($"Can't update data for equipment info {equipmentInfo.Id}", ex);
        //        throw;
        //    }
        //}

        public async Task<IList<Alert>> InitiateCreditCheck(int contractId)
        {
            try
            {
                return
                    await
                        Client.PutAsync<string, IList<Alert>>(
                            $"{_fullUri}/InitiateCreditCheck?contractId={contractId}", "");
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
                    await Client.PutAsync<SignatureUsersDTO, IList<Alert>>($"{_fullUri}/InitiateDigitalSignature", signatureUsers);
            }
            catch (Exception ex)
            {
                this._loggingService.LogError($"Can't initiate digital signature for contract {signatureUsers.ContractId}", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> SubmitContract(int contractId)
        {
            try
            {
                return
                    await
                        Client.PostAsync<string, IList<Alert>>(
                            $"{_fullUri}/SubmitContract?contractId={contractId}", "");               
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
                        Client.GetAsync<Tuple<CreditCheckDTO, IList<Alert>>>(
                            $"{_fullUri}/GetCreditCheckResult?contractId={contractId}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't get credit check result for contract {contractId}", ex);
                throw;
            }
        }

        public async Task<IList<Alert>> GetContractAgreement(int contractId)
        {
            try
            {
                return
                    await
                        Client.GetAsync<IList<Alert>>(
                            $"{_fullUri}/GetContractAgreement?contractId={contractId}");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get contract print agreement", ex);
                throw;
            }
        }

        public async Task<IList<FlowingSummaryItemDTO>> GetContractsSummary(string summaryType)
        {
            try
            {            
                IList<FlowingSummaryItemDTO> result = await Client.GetAsync<IList<FlowingSummaryItemDTO>>($"{_fullUri}/{summaryType}/ContractsSummary");
                return result;
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't get credit contracts summary", ex);
                throw;
            }
        }       

        public async Task<byte[]> GetXlsxReport(IEnumerable<int> ids)
        {
            try
            {
                var response = await Client.PostAsyncWithHttpResponse($"{_fullUri}/CreateXlsxReport", ids);
                return await response.Content.ReadAsByteArrayAsync();
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
                return
                    await Client.PutAsync<ContractDocumentDTO, Tuple<int?, IList<Alert>>>($"{_fullUri}/AddDocument", document);
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
                        Client.PostAsync<string, IList<Alert>>(
                            $"{_fullUri}/RemoveDocument?documentId={documentId}", "");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove contract document", ex);
                throw;
            }
        }

        public async Task<Tuple<int?, IList<Alert>>> AddComment(CommentDTO comment)
        {
            try
            {
                return
                    await
                        Client.PostAsync<CommentDTO, Tuple<int?, IList<Alert>>>(
                            $"{_fullUri}/AddComment", comment);
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
                        Client.PostAsync<string, IList<Alert>>(
                            $"{_fullUri}/RemoveComment?commentId={commentId}", "");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Can't remove comment to contract", ex);
                throw;
            }
        }
    }
}
