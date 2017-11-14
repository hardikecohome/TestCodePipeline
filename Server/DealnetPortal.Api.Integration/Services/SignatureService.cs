using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Practices.ObjectBuilder2;

using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.Services.Signature;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Aspire.Integration.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
using FormField = DealnetPortal.Api.Models.Signature.FormField;

namespace DealnetPortal.Api.Integration.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly ISignatureEngine _signatureEngine;

        private readonly IPdfEngine _pdfEngine;

        //private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly IContractRepository _contractRepository;

        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;
        private readonly IDealerRepository _dealerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAspireService _aspireService;
        private readonly IAspireStorageReader _aspireStorageReader;               

        public SignatureService(
            ISignatureEngine signatureEngine, 
            IPdfEngine pdfEngine,
            IContractRepository contractRepository,
            IFileRepository fileRepository, 
            IUnitOfWork unitOfWork, 
            IAspireService aspireService,
            IAspireStorageReader aspireStorageReader,
            ILoggingService loggingService, 
            IDealerRepository dealerRepository)
        {
            _signatureEngine = signatureEngine;
            _pdfEngine = pdfEngine;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _dealerRepository = dealerRepository;
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _aspireService = aspireService;
            _aspireStorageReader = aspireStorageReader;            
        }

        public async Task<IList<Alert>> ProcessContract(int contractId, string ownerUserId,
            SignatureUser[] signatureUsers)
        {
            List<Alert> alerts = new List<Alert>();

            try
            {
                // Get contract
                var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
                if (contract != null)
                {
                    _loggingService.LogInfo($"Started eSignature processing for contract [{contractId}]");

                    signatureUsers = PrepareSignatureUsers(contract, signatureUsers);

                    var logRes = await _signatureEngine.ServiceLogin().ConfigureAwait(false);
                    _signatureEngine.TransactionId = contract.Details?.SignatureTransactionId;
                    _signatureEngine.DocumentId = contract.Details?.SignatureDocumentId;

                    if (logRes.Any(a => a.Type == AlertType.Error))
                    {
                        LogAlerts(alerts);
                        return alerts;
                    }

                    var agrRes = SelectAgreementTemplate(contract, ownerUserId);

                    if (agrRes.Item1 == null || agrRes.Item2.Any(a => a.Type == AlertType.Error))
                    {
                        alerts.AddRange(agrRes.Item2);
                        LogAlerts(alerts);
                        return alerts;
                    }

                    var agreementTemplate = agrRes.Item1;

                    var trRes = await _signatureEngine.InitiateTransaction(contract, agreementTemplate);

                    if (trRes?.Any() ?? false)
                    {
                        alerts.AddRange(trRes);
                    }

                    var templateFields = await _signatureEngine.GetFormfFields();

                    var fields = PrepareFormFields(contract, templateFields?.Item1, ownerUserId);
                    _loggingService.LogInfo($"{fields.Count} fields collected");

                    var insertRes = await _signatureEngine.InsertDocumentFields(fields);

                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        _loggingService.LogWarning($"Fields merged with agreement document with errors");
                        LogAlerts(alerts);
                        return alerts;
                    }

                    insertRes = await _signatureEngine.InsertSignatures(signatureUsers);

                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        _loggingService.LogWarning($"Signature fields inserted into agreement document form with errors");
                        //LogAlerts(alerts);
                        //return alerts;
                    }
                    else
                    {
                        _loggingService.LogInfo(
                            $"Signature fields inserted into agreement document form successefully");
                    }

                    insertRes = await _signatureEngine.SubmitDocument(signatureUsers);

                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        LogAlerts(alerts);
                        return alerts;
                    }
                    //var updateStatus = signatureUsers?.Any() ?? false
                    //    ? SignatureStatus.Sent
                    //    : SignatureStatus.Draft;
                    UpdateContractDetails(contractId, ownerUserId, _signatureEngine.TransactionId,
                        _signatureEngine.DocumentId, null);

                    UpdateSignersInfo(contractId, ownerUserId, signatureUsers);
                    _loggingService.LogInfo(
                        $"Invitations for agreement document form sent successefully. TransactionId: [{_signatureEngine.TransactionId}], DocumentID [{_signatureEngine.DocumentId}]");
                }
                else
                {
                    var errorMsg = $"Can't get contract [{contractId}] for processing";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = "eSignature error",
                        Message = errorMsg
                    });
                    _loggingService.LogError(errorMsg);
                }
            }
            catch(Exception ex)
            {
                _loggingService.LogError($"Failed to initiate a digital signature for contract [{contractId}]", ex);
                throw;
            }

            LogAlerts(alerts);
            return alerts;
        }

        public async Task<Tuple<bool, IList<Alert>>> CheckPrintAgreementAvailable(int contractId, int documentTypeId,
            string ownerUserId)
        {
            bool isAvailable = false;
            List<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                //Check is pdf template in db. In this case we insert fields on place
                var agrRes = documentTypeId != (int) DocumentTemplateType.SignedInstallationCertificate
                    ? SelectAgreementTemplate(contract, ownerUserId)
                    : SelectInstallCertificateTemplate(contract, ownerUserId);
                if (agrRes?.Item1?.TemplateDocument?.TemplateBinary != null)
                {
                    isAvailable = true;
                }
                else
                {
                    //else we try to get contract from eSignature agreement template
                    //check is agreement created
                    if (!string.IsNullOrEmpty(contract.Details.SignatureTransactionId))
                    {
                        isAvailable = true;
                    }
                    else
                    {
                        // create draft agreement
                        var createAlerts = await ProcessContract(contractId, ownerUserId, null).ConfigureAwait(false);
                        //var docAlerts = await _signatureEngine.CreateDraftDocument(null);
                        isAvailable = true;
                        if (createAlerts.Any())
                        {
                            alerts.AddRange(createAlerts);
                            if (createAlerts.Any(a => a.Type == AlertType.Error))
                            {
                                isAvailable = false;
                            }
                        }
                    }
                }
            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }
            LogAlerts(alerts);


            return new Tuple<bool, IList<Alert>>(isAvailable, alerts);
        }

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetPrintAgreement(int contractId, string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();
            AgreementDocument document = null;

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                //Check is pdf template in db. In this case we insert fields on place
                var agrRes = SelectAgreementTemplate(contract, ownerUserId);

                if (agrRes?.Item1?.TemplateDocument?.TemplateBinary != null)
                {
                    MemoryStream ms = new MemoryStream(agrRes.Item1.TemplateDocument.TemplateBinary, true);

                    var formFields = _pdfEngine.GetFormfFields(ms);

                    var fields = PrepareFormFields(contract, formFields?.Item1, ownerUserId);
                    var insertRes = _pdfEngine.InsertFormFields(ms, fields);

                    if (insertRes?.Item2?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes.Item2);
                    }

                    if (insertRes?.Item1 != null)
                    {
                        var buf = new byte[insertRes.Item1.Length];
                        await insertRes.Item1.ReadAsync(buf, 0, (int) insertRes.Item1.Length);
                        document = new AgreementDocument()
                        {
                            DocumentRaw = buf,
                            Name = agrRes.Item1.TemplateDocument.TemplateName
                        };

                        ReformatTempalteNameWithId(document, contract.Details.TransactionId);
                    }
                }
                else
                {
                    return await GetContractAgreement(contractId, ownerUserId).ConfigureAwait(false);
                }
            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });

                _loggingService.LogError(errorMsg);
            }

            LogAlerts(alerts);

            return new Tuple<AgreementDocument, IList<Alert>>(document, alerts);
        }

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetInstallCertificate(int contractId,
            string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();
            AgreementDocument document = null;

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                //Check is pdf template in db. In this case we insert fields on place
                var agrRes = SelectInstallCertificateTemplate(contract, ownerUserId);
                if (agrRes?.Item1?.TemplateDocument?.TemplateBinary != null)
                {
                    MemoryStream ms = new MemoryStream(agrRes.Item1.TemplateDocument.TemplateBinary, true);

                    var templateFields = _pdfEngine.GetFormfFields(ms);

                    var fields = new List<FormField>();
                    FillHomeOwnerFields(fields, templateFields?.Item1, contract);
                    FillApplicantsFields(fields, contract);
                    FillEquipmentFields(fields, contract, ownerUserId);
                    FillDealerFields(fields, contract);
                    FillInstallCertificateFields(fields, contract);

                    var insertRes = _pdfEngine.InsertFormFields(ms, fields);
                    if (insertRes?.Item2?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes.Item2);
                    }
                    if (insertRes?.Item1 != null)
                    {
                        var buf = new byte[insertRes.Item1.Length];
                        await insertRes.Item1.ReadAsync(buf, 0, (int) insertRes.Item1.Length);
                        document = new AgreementDocument()
                        {
                            DocumentRaw = buf,
                            Name = agrRes.Item1.TemplateDocument.TemplateName
                        };

                        ReformatTempalteNameWithId(document, contract.Details?.TransactionId);
                    }
                }
                else
                {
                    var errorMsg = $"Cannot find installation certificate template for contract [{contractId}]";
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = "eSignature error",
                        Message = errorMsg
                    });
                }
            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });
            }
            LogAlerts(alerts);
            return new Tuple<AgreementDocument, IList<Alert>>(document, alerts);
        }

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId,
            string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();
            AgreementDocument document = null;

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                //check is agreement created
                if (!string.IsNullOrEmpty(contract.Details.SignatureTransactionId))
                {
                    var logRes = await _signatureEngine.ServiceLogin().ConfigureAwait(false);
                    if (logRes.Any(a => a.Type == AlertType.Error))
                    {
                        LogAlerts(alerts);
                        return new Tuple<AgreementDocument, IList<Alert>>(document, alerts);
                    }

                    _signatureEngine.TransactionId = contract.Details.SignatureTransactionId;
                    _signatureEngine.DocumentId = contract.Details.SignatureDocumentId;
                }
                else
                {
                    // create draft agreement
                    var createAlerts = await ProcessContract(contractId, ownerUserId, null).ConfigureAwait(false);
                    //var docAlerts = await _signatureEngine.CreateDraftDocument(null);
                    if (createAlerts.Any())
                    {
                        alerts.AddRange(createAlerts);
                    }
                }

                var docResult = await _signatureEngine.GetDocument(DocumentVersion.Draft).ConfigureAwait(false);
                document = docResult.Item1;

                ReformatTempalteNameWithId(document, contract.Details?.TransactionId);

                if (docResult.Item2.Any())
                {
                    alerts.AddRange(docResult.Item2);
                }
            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Code = ErrorCodes.CantGetContractFromDb,
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }
            LogAlerts(alerts);
            return new Tuple<AgreementDocument, IList<Alert>>(document, alerts);
        }

        public IList<Alert> GetSignatureResults(int contractId, string ownerUserId)
        {
            throw new NotImplementedException();
            //List<Alert> alerts = new List<Alert>();
            //var logRes = LoginToService();
            //if (logRes.Any(a => a.Type == AlertType.Error))
            //{
            //    LogAlerts(alerts);
            //    return alerts;
            //}

            //return alerts;
        }

        public SignatureStatus GetSignatureStatus(int contractId, string ownerUserId)
        {
            SignatureStatus status = SignatureStatus.NotInitiated;
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                status = contract.Details.SignatureStatus ?? SignatureStatus.NotInitiated;
            }
            return status;
        }

        public async Task<IList<Alert>> CancelSignatureProcess(int contractId, string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                _loggingService.LogInfo($"Started cancelling eSignature for contract [{contractId}]");
                    
                var logRes = await _signatureEngine.ServiceLogin().ConfigureAwait(false);
                _signatureEngine.TransactionId = contract.Details?.SignatureTransactionId;
                _signatureEngine.DocumentId = contract.Details?.SignatureDocumentId;
                alerts.AddRange(logRes);

                if (alerts.All(a => a.Type != AlertType.Error))
                {
                    var cancelRes = await _signatureEngine.CancelSignature();
                    alerts.AddRange(cancelRes);
                }
                if (alerts.All(a => a.Type != AlertType.Error))
                {
                    _loggingService.LogInfo($"eSignature envelope {contract?.Details?.SignatureTransactionId} canceled for contract [{contractId}]");
                }
                else
                {
                    LogAlerts(alerts);
                }
            }
            else
            {
                var errorMsg = $"Can't get contract [{contractId}] for processing";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "eSignature error",
                    Message = errorMsg
                });
                _loggingService.LogError(errorMsg);
            }

            return alerts;
        }

        public Task<IList<Alert>> UpdateSignatureUsers(int contractId, string ownerUserId, SignatureUser[] signatureUsers,
            bool reSend = false)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<Alert>> ProcessSignatureEvent(string notificationMsg)
        {
            var docuSignResipientStatuses = new string[] {"Created", "Sent", "Delivered", "Signed", "Declined"};
            var alerts = new List<Alert>();
            try
            {
                XDocument xDocument = XDocument.Parse(notificationMsg);
                var xmlns = xDocument?.Root?.Attribute(XName.Get("xmlns"))?.Value ?? "http://www.docusign.net/API/3.0";

                var envelopeStatusSection = xDocument?.Root?.Element(XName.Get("EnvelopeStatus", xmlns));
                var envelopeId = envelopeStatusSection?.Element(XName.Get("EnvelopeID", xmlns))?.Value;

                var contract = _contractRepository.FindContractBySignatureId(envelopeId);
                if (contract != null && envelopeStatusSection != null)
                {
                    bool updated = false;
                    var envelopeStatus = envelopeStatusSection?.Element(XName.Get("Status", xmlns))?.Value;
                    if (!string.IsNullOrEmpty(envelopeStatus))
                    {
                        _loggingService.LogInfo($"Recieved DocuSign {envelopeStatus} status for envelope {envelopeId}");
                        var envelopeStatusTimeValue = envelopeStatusSection?.Element(XName.Get(envelopeStatus, xmlns))?.Value;
                        DateTime envelopeStatusTime;
                        if (!DateTime.TryParse(envelopeStatusTimeValue, out envelopeStatusTime))
                        {
                            envelopeStatusTime = DateTime.Now;
                        }
                        updated = await ProcessSignatureStatus(contract, envelopeStatus, envelopeStatusTime);
                    }

                    //proceed with recipients statuses
                    var recipientStatusesSection = envelopeStatusSection?.Element(XName.Get("RecipientStatuses", xmlns));
                    if (recipientStatusesSection != null)
                    {
                        var recipientStatuses = recipientStatusesSection.Descendants(XName.Get("RecipientStatus", xmlns));
                        recipientStatuses.ForEach(rs =>
                        {
                            var rsStatus = rs.Element(XName.Get("Status", xmlns))?.Value;
                            var rsLastStatusTime = rs.Elements().Where(rse =>
                                    docuSignResipientStatuses.Any(ds => rse.Name.LocalName.Contains(ds)))
                                .Select(rse =>
                                {
                                    DateTime statusTime;
                                    if (!DateTime.TryParse(rse.Value, out statusTime))
                                    {
                                        statusTime = new DateTime();
                                    }
                                    return statusTime;
                                }).OrderByDescending(rst => rst).FirstOrDefault();
                            var rsName = rs.Element(XName.Get("UserName", xmlns))?.Value;
                            var rsEmail = rs.Element(XName.Get("Email", xmlns))?.Value;
                            var rsComment = rs.Element(XName.Get("DeclineReason", xmlns))?.Value;
                            updated |= ProcessSignerStatus(contract, rsName, rsEmail, rsStatus, rsComment, rsLastStatusTime);
                        });
                    }
                    if (updated)
                    {
                        _unitOfWork.Save();
                    }
                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Code = ErrorCodes.CantGetContractFromDb,
                        Header = "Cannot find contract",
                        Message = $"Cannot find contract for signature envelopeId {envelopeId}"
                    });
                }
            }
            catch (Exception ex)
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,                   
                    Header = "Cannot parse DocuSign notification",
                    Message = $"Error occurred during parsing request from DocuSign: {ex.ToString()}"
                });
                _loggingService.LogError("Error occurred during parsing request from DocuSign", ex);
            }
            return await Task.FromResult(alerts);
        }

        #region private  

        private SignatureUser[] PrepareSignatureUsers(Contract contract, SignatureUser[] signatureUsers)
        {
            List<SignatureUser> usersForProcessing = new List<SignatureUser>();
            var suHomeOwner = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.HomeOwner);
            var homeOwner = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.HomeOwner)
                                ?? new SignatureUser() { Role = SignatureRole.HomeOwner };
            homeOwner.FirstName = !string.IsNullOrEmpty(homeOwner.FirstName) ? homeOwner.FirstName : contract?.PrimaryCustomer?.FirstName;
            homeOwner.LastName = !string.IsNullOrEmpty(homeOwner.LastName) ? homeOwner.LastName : contract?.PrimaryCustomer?.LastName;
            homeOwner.CustomerId = homeOwner.CustomerId ?? contract?.PrimaryCustomer?.Id;
            homeOwner.EmailAddress = !string.IsNullOrEmpty(homeOwner.EmailAddress)
                ? homeOwner.EmailAddress
                : contract?.PrimaryCustomer?.Emails.FirstOrDefault()?.EmailAddress;
            usersForProcessing.Add(homeOwner);

            contract?.SecondaryCustomers?.ForEach(cc =>
            {
                var su = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.AdditionalApplicant &&
                                                   (cc.Id == u.CustomerId) ||
                                                   (cc.FirstName == u.FirstName && cc.LastName == u.LastName));
                var coBorrower = su ?? new SignatureUser() { Role = SignatureRole.AdditionalApplicant };
                coBorrower.FirstName = !string.IsNullOrEmpty(coBorrower.FirstName) ? coBorrower.FirstName : cc.FirstName;
                coBorrower.LastName = !string.IsNullOrEmpty(coBorrower.LastName) ? coBorrower.LastName : cc.LastName;
                coBorrower.CustomerId = coBorrower.CustomerId ?? cc.Id;
                coBorrower.EmailAddress = !string.IsNullOrEmpty(coBorrower.EmailAddress)
                    ? coBorrower.EmailAddress
                    : cc.Emails.FirstOrDefault()?.EmailAddress;
                usersForProcessing.Add(coBorrower);
            });

            var dealerUser = signatureUsers.FirstOrDefault(u => u.Role == SignatureRole.Dealer);
            if (dealerUser != null)
            {
                var dealer = contract.Dealer;
                if (dealer != null)
                {
                    dealerUser.LastName = dealer.UserName;                    
                }
                usersForProcessing.Add(dealerUser);
            }
            return usersForProcessing.ToArray();
        }

        private void UpdateSignersInfo(int contractId, string ownerUserId, SignatureUser[] signatureUsers)
        {
            try
            {
                var signers = AutoMapper.Mapper.Map<ContractSigner[]>(signatureUsers);
                if (signers?.Any() == true)
                {
                    _contractRepository.AddOrUpdateContractSigners(contractId, signers, ownerUserId);
                    _unitOfWork.Save();
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Error on update signers for contract {contractId}", ex);
            }            
        }

        private void UpdateContractDetails(int contractId, string ownerUserId, string transactionId, string dpId,
            SignatureStatus? status)
        {
            try
            {
                var contract = _contractRepository.GetContract(contractId, ownerUserId);
                if (contract != null)
                {
                    bool updated = false;
                    if (!string.IsNullOrEmpty(transactionId))
                    {
                        contract.Details.SignatureTransactionId = transactionId;
                        updated = true;
                    }

                    if (!string.IsNullOrEmpty(dpId))
                    {
                        contract.Details.SignatureDocumentId = dpId;
                        updated = true;
                    }

                    if (status.HasValue)
                    {
                        contract.Details.SignatureStatus = status.Value;
                        updated = true;
                    }

                    if (updated)
                    {
                        contract.Details.SignatureInitiatedTime = DateTime.Now;
                        contract.Details.SignatureLastUpdateTime = DateTime.Now;
                        _unitOfWork.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error on update contract details", ex);
            }
        }

        private async Task<bool> ProcessSignatureStatus(Contract contract, string signatureStatus, DateTime updateTime)
        {
            bool updated = false;            
            var status = ParseSignatureStatus(signatureStatus);
            
            switch (status)
            {
                case SignatureStatus.Completed:
                    //upload doc from docuSign to Aspire
                    updated |= await TransferSignedContractAgreement(contract);
                    break;
            }

            if (status.HasValue && contract.Details.SignatureStatus != status)
            {
                contract.Details.SignatureStatus = status;
                updated = true;
            }
            if (contract.Details.SignatureStatusQualifier != signatureStatus)
            {
                contract.Details.SignatureStatusQualifier = signatureStatus;
                updated = true;
            }
            if (updated)
            {
                contract.Details.SignatureLastUpdateTime = updateTime;
            }

            return updated;
        }

        private bool ProcessSignerStatus(Contract contract, string userName, string email, string status, string comment, DateTime statusTime)
        {
            bool updated = false;

            var signer =
                contract.Signers?.FirstOrDefault(s => (string.IsNullOrEmpty(s.FirstName) || userName.Contains(s.FirstName)) 
                && (string.IsNullOrEmpty(s.LastName) || userName.Contains(s.LastName)));
            //if (signer == null)
            //{
            //    signer = new ContractSigner()
            //    {
            //        Contract = contract,
            //        ContractId = contract.Id,
            //        EmailAddress = email,
            //    };
            //    contract.Signers.Add(signer);
            //    updated = true;
            //}
            if (signer != null)
            {
                var sStatus = ParseSignatureStatus(status);
                if (sStatus != null && signer.SignatureStatus != sStatus)
                {
                    signer.SignatureStatus = sStatus;
                    signer.SignatureStatusQualifier = status;
                    signer.StatusLastUpdateTime = statusTime;
                    if (!string.IsNullOrEmpty(comment))
                    {
                        signer.Comment = comment;
                    }
                    updated = true;
                }
            }

            return updated;
        }

        private async Task<bool> TransferSignedContractAgreement(Contract contract)
        {
            bool updated = false;
            try
            {            
                _signatureEngine.TransactionId = contract.Details.SignatureTransactionId;
                var docResult = await _signatureEngine.GetDocument(DocumentVersion.Signed);
                if (docResult?.Item1 != null)
                {
                    _loggingService.LogInfo(
                        $"Signer contract {docResult.Item1.Name} for contract {contract.Id} was downloaded from DocuSign");
                    ContractDocumentDTO document = new ContractDocumentDTO()
                    {
                        ContractId = contract.Id,
                        CreationDate = DateTime.Now,
                        DocumentTypeId = (int)DocumentTemplateType.SignedContract, // Signed contract !!
                        DocumentName = docResult.Item1.Name,
                        DocumentBytes = docResult.Item1.DocumentRaw
                    };
                    _contractRepository.AddDocumentToContract(contract.Id, AutoMapper.Mapper.Map<ContractDocument>(document), contract.DealerId);
                    updated = true;
                    var alerts = await _aspireService.UploadDocument(contract.Id, document, contract.DealerId);
                    if (alerts?.All(a => a.Type != AlertType.Error) == true)
                    {
                        _loggingService.LogInfo(
                            $"Signer contract {docResult.Item1.Name} for contract {contract.Id} uploaded to Aspire successfully");                    
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError(
                    $"Cannot transfer signed contract from DocuSign for contract {contract.Id} ", ex);
            }
            return updated;
        }

        private SignatureStatus? ParseSignatureStatus(string status)
        {
            SignatureStatus? signatureStatus = null;
            switch (status?.ToLowerInvariant())
            {
                case "created":
                    signatureStatus = SignatureStatus.Created;
                    break;
                case "sent":
                    signatureStatus = SignatureStatus.Sent;
                    break;
                case "delivered":
                    signatureStatus = SignatureStatus.Delivered;
                    break;
                case "signed":
                    signatureStatus = SignatureStatus.Signed;
                    break;
                case "completed":
                    signatureStatus = SignatureStatus.Completed;
                    break;
                case "declined":
                    signatureStatus = SignatureStatus.Declined;
                    break;
                case "deleted":
                    signatureStatus = SignatureStatus.Deleted;
                    break;
            }
            return signatureStatus;
        }

        private void LogAlerts(IList<Alert> alerts)
        {
            alerts.ForEach(a =>
            {
                switch (a.Type)
                {
                    case AlertType.Error:
                        _loggingService.LogError(a.Message);
                        break;
                    case AlertType.Warning:
                        _loggingService.LogWarning(a.Message);
                        break;
                    case AlertType.Information:
                        _loggingService.LogInfo(a.Message);
                        break;
                }
            });
        }

        private List<FormField> PrepareFormFields(Contract contract, IList<FormField> templateFields, string ownerUserId)
        {
            var fields = new List<FormField>();

            FillHomeOwnerFields(fields, templateFields, contract);
            FillApplicantsFields(fields, contract);
            FillEquipmentFields(fields, contract, ownerUserId);
            FillPaymentFields(fields, contract);
            FillDealerFields(fields, contract);

            return fields;
        }


        private Tuple<AgreementTemplate, IList<Alert>> SelectAgreementTemplate(Contract contract,
            string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var agreementType = contract.Equipment.AgreementType;
            var equipmentType = contract.Equipment.NewEquipment?.First()?.Type;

            var province =
                contract.PrimaryCustomer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)
                    ?.State?.ToProvinceCode();

            var dealerTemplates = _fileRepository.FindAgreementTemplates(at =>
                (at.DealerId == contractOwnerId) && (!at.DocumentTypeId.HasValue ||
                                                     at.DocumentTypeId == (int) DocumentTemplateType.SignedContract));

            if (!string.IsNullOrEmpty(contract.ExternalSubDealerName) || dealerTemplates?.Any() != true)
            {
                var extTemplates = _fileRepository.FindAgreementTemplates(at => !string.IsNullOrEmpty(at.ExternalDealerName) &&
                    (at.ExternalDealerName == contract.ExternalSubDealerName || at.ExternalDealerName == contract.Dealer.UserName) &&
                    (!at.DocumentTypeId.HasValue || at.DocumentTypeId == (int) DocumentTemplateType.SignedContract));
                if (extTemplates?.Any() == true && dealerTemplates == null)
                {
                    dealerTemplates = new List<AgreementTemplate>();
                    extTemplates.ForEach(et => dealerTemplates.Add(et));
                }                
            }

            if (!dealerTemplates?.Any() ?? true)
            {
                var parentDealerId = _dealerRepository.GetParentDealerId(contractOwnerId);
                if (!string.IsNullOrEmpty(parentDealerId))
                {
                    dealerTemplates = _fileRepository.FindAgreementTemplates(at =>
                        (at.DealerId == parentDealerId) && (!at.DocumentTypeId.HasValue ||
                                                            at.DocumentTypeId ==
                                                            (int) DocumentTemplateType.SignedContract));
                }
            }

            if (dealerTemplates?.Any() != true)
            {
                //otherwise select any common template
                var commonTemplates =
                    _fileRepository.FindAgreementTemplates(
                        at => string.IsNullOrEmpty(at.DealerId) && string.IsNullOrEmpty(at.ExternalDealerName) &&
                              (!at.DocumentTypeId.HasValue ||
                               at.DocumentTypeId == (int)DocumentTemplateType.SignedContract));
                dealerTemplates = commonTemplates;
            }

            // get agreement template 
            AgreementTemplate agreementTemplate = null;

            if (dealerTemplates?.Any() ?? false)
            {
                // if dealer has templates, select one
                agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                    (agreementType == at.AgreementType) || (at.AgreementType.HasValue && at.AgreementType.Value.HasFlag(agreementType) && agreementType != AgreementType.LoanApplication)
                    && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false))
                    && (string.IsNullOrEmpty(equipmentType) || (at.EquipmentType?.Contains(equipmentType) ?? false)));

                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                        (!at.AgreementType.HasValue || (agreementType == at.AgreementType) || (at.AgreementType.Value.HasFlag(agreementType) && agreementType != AgreementType.LoanApplication))
                        && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false))
                        && (string.IsNullOrEmpty(equipmentType) ||
                            (at.EquipmentType?.Contains(equipmentType) ?? false)));
                }

                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                        (agreementType == at.AgreementType) || (at.AgreementType.HasValue && at.AgreementType.Value.HasFlag(agreementType) && agreementType != AgreementType.LoanApplication)
                        && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false)));
                }

                if (agreementTemplate == null)
                {
                    agreementTemplate =
                        dealerTemplates.FirstOrDefault(at => (agreementType == at.AgreementType) || (at.AgreementType.HasValue && at.AgreementType.Value.HasFlag(agreementType) && agreementType != AgreementType.LoanApplication));
                }
                
            }
            //else
            //{
            //    //otherwise select any common template
            //    var commonTemplates =
            //        _fileRepository.FindAgreementTemplates(
            //            at => string.IsNullOrEmpty(at.DealerId) && string.IsNullOrEmpty(at.ExternalDealerName) &&
            //                  (!at.DocumentTypeId.HasValue ||
            //                   at.DocumentTypeId == (int) DocumentTemplateType.SignedContract));
            //    if (commonTemplates?.Any() ?? false)
            //    {
            //        agreementTemplate =
            //            commonTemplates.FirstOrDefault(at => at.AgreementType.HasValue && at.AgreementType.Value.HasFlag(contract.Equipment.AgreementType));                   
            //    }
            //}

            if (agreementTemplate == null)
            {
                var errorMsg =
                    $"Can't find agreement template for a dealer contract {contractOwnerId} with province = {province} and type = {agreementType}";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Can't find agreement template",
                    Message = errorMsg
                });
            }

            return new Tuple<AgreementTemplate, IList<Alert>>(agreementTemplate, alerts);
        }

        private Tuple<AgreementTemplate, IList<Alert>> SelectInstallCertificateTemplate(Contract contract,
            string contractOwnerId)
        {
            var alerts = new List<Alert>();
            AgreementTemplate agreementTemplate = null;

            var dealerCertificates = _fileRepository.FindAgreementTemplates(at =>
                (at.DealerId == contractOwnerId) && (at.DocumentTypeId ==
                                                     (int) DocumentTemplateType.SignedInstallationCertificate));
            if (dealerCertificates?.Any() ?? false)
            {
                agreementTemplate = dealerCertificates.FirstOrDefault(
                                        cert => (contract.Equipment.AgreementType == cert.AgreementType) || (cert.AgreementType.HasValue && cert.AgreementType.Value.HasFlag(contract.Equipment.AgreementType) && contract.Equipment.AgreementType != AgreementType.LoanApplication))
                                    ?? dealerCertificates.FirstOrDefault();
            }
            else
            {
                var daeler = _contractRepository.GetDealer(contractOwnerId);
                if (daeler != null)
                {
                    var appCertificates = _fileRepository.FindAgreementTemplates(at =>
                        (at.ApplicationId == daeler.ApplicationId) &&
                        (at.DocumentTypeId == (int) DocumentTemplateType.SignedInstallationCertificate));
                    if (appCertificates?.Any() ?? false)
                    {
                        agreementTemplate = appCertificates.FirstOrDefault(
                                                cert => (contract.Equipment.AgreementType == cert.AgreementType) || (cert.AgreementType.HasValue && cert.AgreementType.Value.HasFlag(contract.Equipment.AgreementType) && contract.Equipment.AgreementType != AgreementType.LoanApplication))
                                            ?? appCertificates.FirstOrDefault();
                    }
                }
            }

            if (agreementTemplate == null)
            {
                var errorMsg =
                    $"Can't find installation certificate template for a dealer contract {contractOwnerId}";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Can't find installation certificate template",
                    Message = errorMsg
                });
            }

            return new Tuple<AgreementTemplate, IList<Alert>>(agreementTemplate, alerts);
        }

        private bool LogoutFromService()
        {
            return true;
            //return _signatureServiceAgent.Logout().GetAwaiter().GetResult();
        }

        private void FillHomeOwnerFields(List<FormField> formFields, IList<FormField> templateFields, Contract contract)
        {
            if (contract.Details.TransactionId != null)
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ApplicationId,
                    Value = contract.Details.TransactionId
                });
            }
            if (contract.PrimaryCustomer != null)
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.FirstName,
                    Value = contract.PrimaryCustomer.FirstName
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.LastName,
                    Value = contract.PrimaryCustomer.LastName
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.DateOfBirth,
                    Value = contract.PrimaryCustomer.DateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = "ID1",
                    Value = contract.PrimaryCustomer.DealerInitial
                });

                if (contract.PrimaryCustomer.VerificationIdName == "Driver’s license")
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = "Tiv1",
                        Value = "true"
                    });                    
                }
                else if (contract.PrimaryCustomer.VerificationIdName != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = "Tiv1Other",
                        Value = "true"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = "OtherID",
                        Value = contract.PrimaryCustomer.VerificationIdName
                    });
                }
                if (contract.PrimaryCustomer.Locations?.Any() ?? false)
                {
                    var mainAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress);
                    if (mainAddress != null)
                    {                        
                        if (!string.IsNullOrEmpty(mainAddress.Unit) && templateFields?.All(tf => tf.Name != PdfFormFields.SuiteNo) == true)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = PdfFormFields.InstallationAddress,
                                Value = $"{mainAddress.Street}, {Resources.Resources.Suite} {mainAddress.Unit}"
                            });
                        }
                        else
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = PdfFormFields.InstallationAddress,
                                Value = mainAddress.Street
                            });
                        }                        
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.City,
                            Value = mainAddress.City
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.Province,
                            Value = mainAddress.State
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.PostalCode,
                            Value = mainAddress.PostalCode
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.SuiteNo,
                            Value = mainAddress.Unit
                        });
                    }
                    var mailAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MailAddress);
                    if (mailAddress != null)
                    {
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.CheckBox,
                            Name = PdfFormFields.IsMailingDifferent,
                            Value = "true"
                        });
                        var sMailAddress = !string.IsNullOrEmpty(mailAddress.Unit) ? 
                                        $"{mailAddress.Street}, {Resources.Resources.Suite} {mailAddress.Unit}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}" 
                                        : $"{mailAddress.Street}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}";
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.MailingAddress,
                            Value = sMailAddress
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.MailingOrPreviousAddress,
                            Value = sMailAddress
                        });
                    }
                    var previousAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.PreviousAddress);
                    if (previousAddress != null)
                    {
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.CheckBox,
                            Name = PdfFormFields.IsPreviousAddress,
                            Value = "true"
                        });
                        var sPrevAddress = !string.IsNullOrEmpty(previousAddress.Unit) ?
                            $"{previousAddress.Street}, {Resources.Resources.Suite} {previousAddress.Unit}, {previousAddress.City}, {previousAddress.State}, {previousAddress.PostalCode}"
                            : $"{previousAddress.Street}, {previousAddress.City}, {previousAddress.State}, {previousAddress.PostalCode}";

                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.PreviousAddress,
                            Value = sPrevAddress
                        });
                        if (mailAddress == null)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = PdfFormFields.MailingOrPreviousAddress,
                                Value = sPrevAddress
                            });
                        }
                    }
                    if (contract.HomeOwners?.Any(ho => ho.Id == contract.PrimaryCustomer.Id) ?? false)
                    {
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.CheckBox,
                            Name = PdfFormFields.IsHomeOwner,
                            Value = "true"
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.CheckBox,
                            Name = $"{PdfFormFields.IsHomeOwner}_2",
                            Value = "true"
                        });
                    }
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.CustomerName,
                        Value = $"{contract.PrimaryCustomer.LastName} {contract.PrimaryCustomer.FirstName}"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = $"{PdfFormFields.CustomerName}2",
                        Value = $"{contract.PrimaryCustomer.LastName} {contract.PrimaryCustomer.FirstName}"
                    });
                }
            }

            if (contract.PrimaryCustomer?.Emails?.Any() ?? false)
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EmailAddress,
                    Value =
                        contract.PrimaryCustomer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)
                            ?.EmailAddress ?? contract.PrimaryCustomer.Emails.First()?.EmailAddress
                });
            }

            if (contract.PrimaryCustomer?.Phones?.Any() ?? false)
            {
                var homePhone = contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
                var cellPhone = contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell);
                var businessPhone =
                    contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business);

                if (homePhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.HomePhone,
                        Value = homePhone.PhoneNum
                    });
                }
                if (cellPhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.CellPhone,
                        Value = cellPhone.PhoneNum
                    });
                }
                if (businessPhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.BusinessPhone,
                        Value = businessPhone.PhoneNum
                    });
                }
                if (businessPhone != null || cellPhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.BusinessOrCellPhone,
                        Value = businessPhone?.PhoneNum ?? cellPhone?.PhoneNum
                    });
                }
            }

            if (!string.IsNullOrEmpty(contract.PrimaryCustomer?.Sin))
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.Sin,
                    Value = contract.PrimaryCustomer.Sin
                });
            }

            if (!string.IsNullOrEmpty(contract.PrimaryCustomer?.DriverLicenseNumber))
            {
                var dl = contract.PrimaryCustomer.DriverLicenseNumber.Replace(" ", "").Replace("-", "");
                for (int ch = 1;
                    ch <= Math.Min(dl.Length, 15);
                    ch++)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = $"{PdfFormFields.Dl}{ch}",
                        Value = $"{dl[ch - 1]}"
                    });
                }
            }
        }

        private void FillApplicantsFields(List<FormField> formFields, Contract contract)
        {
            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                var addApplicant = contract.SecondaryCustomers.First();
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.FirstName2,
                    Value = addApplicant.FirstName
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.LastName2,
                    Value = addApplicant.LastName
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.DateOfBirth2,
                    Value = addApplicant.DateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = "ID2",
                    Value = addApplicant.DealerInitial
                });

                if (addApplicant.VerificationIdName == "Driver’s license")
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = "Tiv2",
                        Value = "true"
                    });
                }
                else if (addApplicant.VerificationIdName != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = "Tiv2Other",
                        Value = "true"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = "OtherID_2",
                        Value = addApplicant.VerificationIdName
                    });
                }


                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EmailAddress2,
                    Value = addApplicant.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ??
                            addApplicant.Emails.First()?.EmailAddress
                });
                if (!string.IsNullOrEmpty(addApplicant?.Sin))
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.Sin2,
                        Value = addApplicant.Sin
                    });
                }
                var homePhone = addApplicant.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
                var cellPhone = addApplicant.Phones?.FirstOrDefault(p => p.PhoneType == PhoneType.Cell);
                if (homePhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.HomePhone2,
                        Value = homePhone.PhoneNum
                    });
                }
                if (cellPhone != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.CellPhone2,
                        Value = cellPhone.PhoneNum
                    });
                }

                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.CustomerName2,
                    Value =
                        $"{contract.SecondaryCustomers.First().LastName} {contract.SecondaryCustomers.First().FirstName}"
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = $"{PdfFormFields.CustomerName2}2",
                    Value =
                        $"{contract.SecondaryCustomers.First().LastName} {contract.SecondaryCustomers.First().FirstName}"
                });

                if (contract.HomeOwners?.Any(ho => ho.Id == addApplicant.Id) ?? false)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.IsHomeOwner2,
                        Value = "true"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = $"{PdfFormFields.IsHomeOwner2}_2",
                        Value = "true"
                    });
                }
            }
        }

        private void FillEquipmentFields(List<FormField> formFields, Contract contract, string ownerUserId)
        {
            if (contract.Equipment?.NewEquipment?.Any() ?? false)
            {
                var newEquipments = contract.Equipment.NewEquipment;
                var fstEq = newEquipments.First();

                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EquipmentQuantity,
                    Value = contract.Equipment.NewEquipment.Count(ne => ne.Type == fstEq.Type).ToString()
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EquipmentCost,
                    Value = contract.Equipment.NewEquipment.Select(ne => ne.Cost).Sum().ToString()
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = $"{PdfFormFields.EquipmentCost}_2",
                    Value = contract.Equipment.NewEquipment.Select(ne => ne.Cost).Sum().ToString()
                });

                if (fstEq != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.EquipmentDescription,
                        Value = fstEq.Description
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.IsEquipment,
                        Value = "true"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.EquipmentMonthlyRental,
                        Value = fstEq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                    });
                }

                var othersEq = new List<NewEquipment>();
                foreach (var eq in newEquipments)
                {
                    switch (eq.Type)
                    {
                        case "ECO1": // Air Conditioner
                            // check is already filled
                            if (!formFields.Exists(f => f.Name == PdfFormFields.IsAirConditioner))
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.CheckBox,
                                    Name = PdfFormFields.IsAirConditioner,
                                    Value = "true"
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.AirConditionerDetails,
                                    Value = eq.Description
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.AirConditionerMonthlyRental,
                                    Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                                });
                            }
                            else
                            {
                                othersEq.Add(eq);
                            }
                            break;
                        case "ECO2": // Boiler
                            if (!formFields.Exists(f => f.Name == PdfFormFields.IsBoiler))
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.CheckBox,
                                    Name = PdfFormFields.IsBoiler,
                                    Value = "true"
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.BoilerDetails,
                                    Value = eq.Description
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.BoilerMonthlyRental,
                                    Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                                });
                            }
                            else
                            {
                                othersEq.Add(eq);
                            }
                            break;
                        case "ECO3": // Doors
                            othersEq.Add(eq);
                            break;
                        case "ECO4": // Fireplace
                            othersEq.Add(eq);
                            break;
                        case "ECO5": // Furnace
                        case "ECO40": // Air Handler - we have common 
                            if (!formFields.Exists(f => f.Name == PdfFormFields.IsFurnace))
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.CheckBox,
                                    Name = PdfFormFields.IsFurnace,
                                    Value = "true"
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.FurnaceDetails,
                                    Value = eq.Description
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.FurnaceMonthlyRental,
                                    Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                                });
                            }
                            else
                            {
                                othersEq.Add(eq);
                            }
                            break;
                        case "ECO6": // HWT
                            othersEq.Add(eq);
                            break;
                        case "ECO7": // Plumbing
                            othersEq.Add(eq);
                            break;
                        case "ECO9": // Roofing
                            othersEq.Add(eq);
                            break;
                        case "ECO10": // Siding
                            othersEq.Add(eq);
                            break;
                        case "ECO11": // Tankless Water Heater
                            othersEq.Add(eq);
                            break;
                        case "ECO13": // Windows
                            othersEq.Add(eq);
                            break;
                        case "ECO38": // Sunrooms
                            othersEq.Add(eq);
                            break;                       
                        case "ECO42": // Flooring
                            othersEq.Add(eq);
                            break;
                        case "ECO43": // Porch Enclosure
                            othersEq.Add(eq);
                            break;
                        case "ECO44": // Water Treatment System
                            if (!formFields.Exists(f => f.Name == PdfFormFields.IsWaterFiltration))
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.CheckBox,
                                    Name = PdfFormFields.IsWaterFiltration,
                                    Value = "true"
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.WaterFiltrationDetails,
                                    Value = eq.Description
                                });
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.WaterFiltrationMonthlyRental,
                                    Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                                });
                            }
                            else
                            {
                                othersEq.Add(eq);
                            }
                            break;
                        case "ECO45": // Heat Pump
                            othersEq.Add(eq);
                            break;
                        case "ECO46": // HRV
                            othersEq.Add(eq);
                            break;
                        case "ECO47": // Bathroom
                            othersEq.Add(eq);
                            break;
                        case "ECO48": // Kitchen
                            othersEq.Add(eq);
                            break;
                        case "ECO49": // Hepa System
                            othersEq.Add(eq);
                            break;
                        case "ECO50": // Unknown
                            othersEq.Add(eq);
                            break;
                        case "ECO52": // Security System
                            othersEq.Add(eq);
                            break;
                        case "ECO55": // Basement Repair
                            othersEq.Add(eq);
                            break;
                    }
                }
                if (othersEq.Any())
                {
                    for (int i = 0; i < othersEq.Count; i++)
                    {
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.CheckBox,
                            Name = $"{PdfFormFields.IsOtherBase}{i + 1}",
                            Value = "true"
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = $"{PdfFormFields.OtherDetailsBase}{i + 1}",
                            Value = othersEq[i].Description
                        });
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = $"{PdfFormFields.OtherMonthlyRentalBase}{i + 1}",
                            Value = othersEq[i].MonthlyCost?.ToString("F", CultureInfo.InvariantCulture)
                        });
                    }
                }
                for (int i = 0; i < newEquipments.Count; i++)
                {
                
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = $"{PdfFormFields.EquipmentQuantity}_{i}",
                        Value = "1"
                    });

                    var eqType = _contractRepository.GetEquipmentTypeInfo(newEquipments.ElementAt(i).Type);
                    string eqTypeDescr = null;
                    if (eqType != null)
                    {
                        eqTypeDescr = ResourceHelper.GetGlobalStringResource(eqType.DescriptionResource) ??
                                          eqType.Description;
                    }                    
                    var eqDescription = !string.IsNullOrEmpty(eqTypeDescr)
                        ? $"{eqTypeDescr} - {newEquipments.ElementAt(i).Description}" : newEquipments.ElementAt(i).Description;                    
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = $"{PdfFormFields.EquipmentDescription}_{i}",
                        Value = eqDescription
                    });                    
                }
                    // support old contracts with EstimatedInstallationDate in Equipment
                if (contract.Equipment.EstimatedInstallationDate.HasValue ||
                    ((contract.Equipment.NewEquipment?.First()?.EstimatedInstallationDate.HasValue) ?? false))
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.InstallDate,
                        Value =
                            contract.Equipment.EstimatedInstallationDate?.ToString("MM/dd/yyyy",
                                CultureInfo.InvariantCulture) ?? contract.Equipment.NewEquipment.First()
                                .EstimatedInstallationDate?.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                    });
                }
            }
            if (contract.Equipment != null)
            {
                var paySummary = _contractRepository.GetContractPaymentsSummary(contract.Id, ownerUserId);

                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.TotalPayment,
                    Value = paySummary.TotalPayment?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.TotalMonthlyPayment,
                    Value = paySummary.MonthlyPayment?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.MonthlyPayment,
                    Value = paySummary.MonthlyPayment?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.Hst,
                    Value = paySummary.Hst?.ToString("F", CultureInfo.InvariantCulture)
                });

                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.RequestedTerm,
                    Value = (contract.Equipment.AgreementType == AgreementType.LoanApplication)
                        ? contract.Equipment.LoanTerm?.ToString()
                        : contract.Equipment.RequestedTerm?.ToString()
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.AmortizationTerm,
                    Value = contract.Equipment.AmortizationTerm?.ToString()
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.DownPayment,
                    Value = contract.Equipment.DownPayment?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.AdmeenFee,
                    Value = contract.Equipment.AdminFee?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.CustomerRate,
                    Value = contract.Equipment.CustomerRate?.ToString("F", CultureInfo.InvariantCulture)
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.CustomerRate2,
                    Value = contract.Equipment.CustomerRate?.ToString("F", CultureInfo.InvariantCulture)
                });

                if (contract.Equipment.AgreementType == AgreementType.LoanApplication &&
                    paySummary?.LoanDetails != null)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.LoanTotalCashPrice,
                        Value = paySummary.LoanDetails.TotalCashPrice.ToString("F", CultureInfo.InvariantCulture)
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.LoanAmountFinanced,
                        Value = paySummary.LoanDetails.TotalAmountFinanced.ToString("F", CultureInfo.InvariantCulture)
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.LoanTotalObligation,
                        Value = paySummary.LoanDetails.TotalObligation.ToString("F", CultureInfo.InvariantCulture)
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.LoanBalanceOwing,
                        Value = paySummary.LoanDetails.ResidualBalance.ToString("F", CultureInfo.InvariantCulture)
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.LoanTotalBorowingCost,
                        Value = paySummary.LoanDetails.TotalBorowingCost.ToString("F", CultureInfo.InvariantCulture)
                    });
                }

                if (contract.Equipment.DeferralType != DeferralType.NoDeferral)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.YesDeferral,
                        Value = "true"
                    });
                    var defTerm = contract.Equipment.DeferralType.GetPersistentEnumDescription().Split()[0];
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.DeferralTerm,
                        Value = defTerm
                    });
                }
                else
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.NoDeferral,
                        Value = "true"
                    });
                }
            }
        }

        private void FillPaymentFields(List<FormField> formFields, Contract contract)
        {
            if (contract.PaymentInfo != null)
            {
                if (contract.PaymentInfo.PaymentType == PaymentType.Enbridge)
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.IsEnbridge,
                        Value = "true"
                    });
                    if (!string.IsNullOrEmpty(contract.PaymentInfo.EnbridgeGasDistributionAccount))
                    {
                        var ean = contract.PaymentInfo.EnbridgeGasDistributionAccount.Replace(" ", "");
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.EnbridgeAccountNumber,
                            Value = ean
                        });
                        for (int ch = 1;
                            ch <= Math.Min(ean.Length, 12);
                            ch++)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = $"{PdfFormFields.Ean}{ch}",
                                Value = $"{ean[ch - 1]}"
                            });
                        }
                    }
                }
                else
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = PdfFormFields.IsPAD,
                        Value = "true"
                    });
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.CheckBox,
                        Name = contract.PaymentInfo.PrefferedWithdrawalDate == WithdrawalDateType.First
                            ? PdfFormFields.IsPAD1
                            : PdfFormFields.IsPAD15,
                        Value = "true"
                    });

                    if (!string.IsNullOrEmpty(contract.PaymentInfo.BlankNumber))
                    {
                        var bn = contract.PaymentInfo.BlankNumber.Replace(" ", "");
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.BankNumber,
                            Value = bn
                        });
                        for (int ch = 1;
                            ch <= Math.Min(bn.Length, 3);
                            ch++)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = $"{PdfFormFields.Bn}{ch}",
                                Value = $"{bn[ch - 1]}"
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(contract.PaymentInfo.TransitNumber))
                    {
                        var tn = contract.PaymentInfo.TransitNumber.Replace(" ", "");
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.TransitNumber,
                            Value = tn
                        });
                        for (int ch = 1;
                            ch <= Math.Min(tn.Length, 5);
                            ch++)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = $"{PdfFormFields.Tn}{ch}",
                                Value = $"{tn[ch - 1]}"
                            });
                        }
                    }
                    if (!string.IsNullOrEmpty(contract.PaymentInfo.AccountNumber))
                    {
                        var an = contract.PaymentInfo.AccountNumber.Replace(" ", "");
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.AccountNumber,
                            Value = an
                        });
                        for (int ch = 1;
                            ch <= Math.Min(an.Length, 12);
                            ch++)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = $"{PdfFormFields.An}{ch}",
                                Value = $"{an[ch - 1]}"
                            });
                        }
                    }
                }
            }
        }

        private void FillDealerFields(List<FormField> formFields, Contract contract)
        {
            if (!string.IsNullOrEmpty(contract?.Equipment?.SalesRep))
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.SalesRep,
                    Value = contract.Equipment.SalesRep
                });
            }

            //try to get Dealer info from Aspire and fill it
            if (!string.IsNullOrEmpty(contract?.Dealer.AspireLogin))
            {
                TimeSpan aspireRequestTimeout = TimeSpan.FromSeconds(5);
                Task timeoutTask = Task.Delay(aspireRequestTimeout);
                var aspireRequestTask =
                    Task.Run(() => _aspireStorageReader.GetDealerInfo(contract?.Dealer.AspireLogin));

                try
                {
                    if (Task.WhenAny(aspireRequestTask, timeoutTask).ConfigureAwait(false).GetAwaiter().GetResult() ==
                        aspireRequestTask)
                    {
                        var dealerInfo = AutoMapper.Mapper.Map<DealerDTO>(aspireRequestTask.Result);

                        if (dealerInfo != null)
                        {
                            formFields.Add(new FormField()
                            {
                                FieldType = FieldType.Text,
                                Name = PdfFormFields.DealerName,
                                Value = dealerInfo.FirstName
                            });

                            var dealerAddress =
                                dealerInfo.Locations?.FirstOrDefault();
                            if (dealerAddress != null)
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.DealerAddress,
                                    Value =
                                        $"{dealerAddress.Street}, {dealerAddress.City}, {dealerAddress.State}, {dealerAddress.PostalCode}"
                                });
                            }

                            if (dealerInfo.Phones?.Any() ?? false)
                            {
                                formFields.Add(new FormField()
                                {
                                    FieldType = FieldType.Text,
                                    Name = PdfFormFields.DealerPhone,
                                    Value = dealerInfo.Phones.First().PhoneNum
                                });
                            }
                        }
                    }
                    else
                    {
                        _loggingService.LogError("Cannot get dealer info from Aspire");
                    }
                }
                catch (Exception ex)
                {
                    _loggingService.LogError("Cannot get dealer info from Aspire database", ex);
                }
            }
        }

        private void FillInstallCertificateFields(List<FormField> formFields, Contract contract)
        {
            formFields.Add(new FormField()
            {
                FieldType = FieldType.Text,
                Name = PdfFormFields.InstallerName,
                Value = $"{contract.Equipment.InstallerLastName} {contract.Equipment.InstallerFirstName}"
            });
            if (contract.Equipment.InstallationDate.HasValue)
            {
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.InstallationDate,
                    Value =
                        contract.Equipment?.InstallationDate.Value.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture)
                });
            }

            if (contract.Equipment.NewEquipment?.Any() ?? false)
            {
                string eqDescs = string.Empty;
                string eqModels = string.Empty;
                string eqSerials = string.Empty;
                contract.Equipment.NewEquipment.ForEach(eq =>
                {
                    if (!string.IsNullOrEmpty(eqModels))
                    {
                        eqModels += "; ";
                    }
                    eqModels += eq.InstalledModel;
                    if (!string.IsNullOrEmpty(eqSerials))
                    {
                        eqSerials += "; ";
                    }
                    eqSerials += eq.InstalledSerialNumber;
                    if (!string.IsNullOrEmpty(eqDescs))
                    {
                        eqDescs += "; ";
                    }
                    eqDescs += eq.Description;
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EquipmentModel,
                    Value = eqModels
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.EquipmentSerialNumber,
                    Value = eqSerials
                });
                var descField = formFields.FirstOrDefault(f => f.Name == PdfFormFields.EquipmentDescription);
                if (descField != null)
                {
                    descField.Value = eqDescs;
                }
                else
                {
                    formFields.Add(new FormField()
                    {
                        FieldType = FieldType.Text,
                        Name = PdfFormFields.EquipmentDescription,
                        Value = eqDescs
                    });
                }
            }

            if (contract.Equipment.ExistingEquipment?.Any() ?? false)
            {
                var exEq = contract.Equipment.ExistingEquipment.First();
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.CheckBox,
                    Name = exEq.IsRental
                        ? PdfFormFields.IsExistingEquipmentRental
                        : PdfFormFields.IsExistingEquipmentNoRental,
                    Value = "true"
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentRentalCompany,
                    Value = exEq.RentalCompany
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentMake,
                    Value = exEq.Make
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentModel,
                    Value = exEq.Model
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentSerialNumber,
                    Value = exEq.SerialNumber
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentGeneralCondition,
                    Value = exEq.GeneralCondition
                });
                formFields.Add(new FormField()
                {
                    FieldType = FieldType.Text,
                    Name = PdfFormFields.ExistingEquipmentAge,
                    Value = exEq.EstimatedAge.ToString("F", CultureInfo.InvariantCulture)
                });
            }
        }

        private void ReformatTempalteNameWithId(AgreementDocument document, string transactionId)
        {
            if (string.IsNullOrEmpty(transactionId))
            {
                return;
            }

            var defaultName = document.Name;
            document.Name = $"{transactionId} {defaultName}";
        }

        #endregion
    }
}