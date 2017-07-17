using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
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
        private readonly IAspireStorageReader _aspireStorageReader;

        private readonly string _eCoreSignatureRole;
        private readonly string _eCoreAgreementTemplate;
        private readonly string _eCoreCustomerSecurityCode;

        private List<string> _signatureFields = new List<string>() {"Signature1", "Signature2"}; //, "Sinature3"};
        private List<string> _signatureRoles = new List<string>();

        public SignatureService(
            ISignatureEngine signatureEngine, 
            IPdfEngine pdfEngine,
            IContractRepository contractRepository,
            IFileRepository fileRepository, 
            IUnitOfWork unitOfWork, 
            IAspireStorageReader aspireStorageReader,
            ILoggingService loggingService, 
            IDealerRepository dealerRepository)
        {
            _signatureEngine = signatureEngine;
            _pdfEngine = pdfEngine;
            //_signatureServiceAgent = signatureServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _dealerRepository = dealerRepository;
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            _aspireStorageReader = aspireStorageReader;

            _eCoreSignatureRole = System.Configuration.ConfigurationManager.AppSettings["eCoreSignatureRole"];
            _eCoreAgreementTemplate = System.Configuration.ConfigurationManager.AppSettings["eCoreAgreementTemplate"];
            _eCoreCustomerSecurityCode =
                System.Configuration.ConfigurationManager.AppSettings["eCoreCustomerSecurityCode"];

            _signatureRoles.Add(_eCoreSignatureRole);
            _signatureRoles.Add($"{_eCoreSignatureRole}2");
            //_signatureRoles.Add($"{_eCoreSignatureRole}3");
        }

        public async Task<IList<Alert>> ProcessContract(int contractId, string ownerUserId,
            SignatureUser[] signatureUsers)
        {
            List<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                _loggingService.LogInfo($"Started eSignature processing for contract [{contractId}]");
                var fields = PrepareFormFields(contract, ownerUserId);
                _loggingService.LogInfo($"{fields.Count} fields collected");

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

                var insertRes = await _signatureEngine.InsertDocumentFields(fields);
                //InsertAgreementFields(docId, fields);

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
                //else
                //{
                //    _loggingService.LogInfo($"Fields merged with agreement document form {docId} successefully");
                //    UpdateContractDetails(contractId, ownerUserId, null, null, SignatureStatus.FieldsMerged);
                //}     

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

                _loggingService.LogInfo(
                    $"Invitations for agreement document form sent successefully. TransactionId: [{_signatureEngine.TransactionId}], DocumentID [{_signatureEngine.DocumentId}]");
                var updateStatus = signatureUsers?.Any() ?? false
                    ? SignatureStatus.InvitationsSent
                    : SignatureStatus.Draft;
                UpdateContractDetails(contractId, ownerUserId, _signatureEngine.TransactionId,
                    _signatureEngine.DocumentId, updateStatus);
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
                if (agrRes?.Item1?.AgreementForm != null)
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

                if (agrRes?.Item1?.AgreementForm != null)
                {
                    MemoryStream ms = new MemoryStream(agrRes.Item1.AgreementForm, true);

                    var fields = PrepareFormFields(contract, ownerUserId);
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
                            Name = agrRes.Item1.TemplateName
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
                if (agrRes?.Item1?.AgreementForm != null)
                {
                    MemoryStream ms = new MemoryStream(agrRes.Item1.AgreementForm, true);

                    var fields = new List<FormField>();
                    FillHomeOwnerFields(fields, contract);
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
                            Name = agrRes.Item1.TemplateName
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
                        contract.Details.SignatureTime = DateTime.Now;
                        _unitOfWork.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error on update contract details", ex);
            }
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

        private List<FormField> PrepareFormFields(Contract contract, string ownerUserId)
        {
            var fields = new List<FormField>();
            //Code to add Application Id

            FillHomeOwnerFields(fields, contract);
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

            if (!string.IsNullOrEmpty(contract.ExternalSubDealerName))
            {
                var extTemplates = _fileRepository.FindAgreementTemplates(at =>
                    (at.ExternalDealerName == contract.ExternalSubDealerName) &&
                    (!at.DocumentTypeId.HasValue || at.DocumentTypeId == (int) DocumentTemplateType.SignedContract));
                extTemplates?.ForEach(et => dealerTemplates.Add(et));
            }

            if (!dealerTemplates?.Any() ?? true)
            {
                var parentDealerId = _dealerRepository.GetParentDealerId(contractOwnerId);
                dealerTemplates = _fileRepository.FindAgreementTemplates(at =>
                    (at.DealerId == parentDealerId) && (!at.DocumentTypeId.HasValue ||
                                                        at.DocumentTypeId ==
                                                        (int) DocumentTemplateType.SignedContract));
            }

            // get agreement template 
            AgreementTemplate agreementTemplate = null;

            if (dealerTemplates?.Any() ?? false)
            {
                // if dealer has templates, select one
                agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                    (at.AgreementType == contract.Equipment.AgreementType)
                    && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false))
                    && (string.IsNullOrEmpty(equipmentType) || (at.EquipmentType?.Contains(equipmentType) ?? false)));

                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                        (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType)
                        && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false))
                        && (string.IsNullOrEmpty(equipmentType) ||
                            (at.EquipmentType?.Contains(equipmentType) ?? false)));
                }

                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                        (at.AgreementType == contract.Equipment.AgreementType)
                        && at.State == province);
                }

                if (agreementTemplate == null)
                {
                    agreementTemplate =
                        dealerTemplates.FirstOrDefault(at => at.AgreementType == contract.Equipment.AgreementType);
                }
                //DEAL-1903 Dealer should not generate contracts that are not specific for his role
                //if (agreementTemplate == null)
                //{
                //    agreementTemplate =
                //        dealerTemplates.FirstOrDefault(at => string.IsNullOrEmpty(at.State) || at.State == province);
                //}

                //if (agreementTemplate == null)
                //{
                //    agreementTemplate = dealerTemplates.First();
                //}
            }
            else
            {
                //otherwise select any common template
                var commonTemplates =
                    _fileRepository.FindAgreementTemplates(
                        at => string.IsNullOrEmpty(at.DealerId) &&
                              (!at.DocumentTypeId.HasValue ||
                               at.DocumentTypeId == (int) DocumentTemplateType.SignedContract));
                if (commonTemplates?.Any() ?? false)
                {
                    agreementTemplate =
                        commonTemplates.FirstOrDefault(at => at.AgreementType == contract.Equipment.AgreementType);

                    //if (agreementTemplate == null)
                    //{
                    //    agreementTemplate = commonTemplates.FirstOrDefault(at => at.State == province);
                    //}

                    //if (agreementTemplate == null)
                    //{
                    //    agreementTemplate = commonTemplates.FirstOrDefault(at => at.AgreementForm != null);
                    //}
                }
            }

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
                                        cert => cert.AgreementType == contract.Equipment.AgreementType)
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
                                                cert => cert.AgreementType == contract.Equipment.AgreementType)
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

        private void FillHomeOwnerFields(List<FormField> formFields, Contract contract)
        {
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
                if (contract.PrimaryCustomer.Locations?.Any() ?? false)
                {
                    var mainAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress);
                    if (mainAddress != null)
                    {
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.InstallationAddress,
                            Value = mainAddress.Street
                        });
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
                        formFields.Add(new FormField()
                        {
                            FieldType = FieldType.Text,
                            Name = PdfFormFields.MailingAddress,
                            Value =
                                $"{mailAddress.Street}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}"
                        });
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
                        FieldType = FieldType.CheckBox,
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
                        case "ECO40": // Air Handler
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
    }
}