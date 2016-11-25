using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Api.Integration.Services.Signature;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;
using FormField = DealnetPortal.Api.Models.Signature.FormField;

namespace DealnetPortal.Api.Integration.Services
{    
    public class SignatureService : ISignatureService
    {
        private readonly ISignatureEngine _signatureEngine;
        private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;
        private readonly IUnitOfWork _unitOfWork;
        
        private readonly string _eCoreSignatureRole;
        private readonly string _eCoreAgreementTemplate;
        private readonly string _eCoreCustomerSecurityCode;

        private List<string> _signatureFields = new List<string>() {"Signature1", "Signature2"};//, "Sinature3"};
        private List<string> _signatureRoles = new List<string>();

        public SignatureService(ISignatureEngine signatureEngine, IESignatureServiceAgent signatureServiceAgent, IContractRepository contractRepository,
            IFileRepository fileRepository, IUnitOfWork unitOfWork, ILoggingService loggingService)
        {
            _signatureEngine = signatureEngine;
            _signatureServiceAgent = signatureServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _fileRepository = fileRepository;
            _unitOfWork = unitOfWork;
            
            _eCoreSignatureRole = System.Configuration.ConfigurationManager.AppSettings["eCoreSignatureRole"];
            _eCoreAgreementTemplate = System.Configuration.ConfigurationManager.AppSettings["eCoreAgreementTemplate"];
            _eCoreCustomerSecurityCode = System.Configuration.ConfigurationManager.AppSettings["eCoreCustomerSecurityCode"];

            _signatureRoles.Add(_eCoreSignatureRole);
            _signatureRoles.Add($"{_eCoreSignatureRole}2");
            //_signatureRoles.Add($"{_eCoreSignatureRole}3");
        }

        public async Task<IList<Alert>> ProcessContract(int contractId, string ownerUserId, SignatureUser[] signatureUsers)
        {
            List<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                _loggingService.LogInfo($"Started eSignature processing for contract [{contractId}]");
                var fields = PrepareFormFields(contract);
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
                
                _loggingService.LogInfo($"Invitations for agreement document form sent successefully. TransactionId: [{_signatureEngine.TransactionId}], DocumentID [{_signatureEngine.DocumentId}]");
                var updateStatus = signatureUsers?.Any() ?? false
                    ? SignatureStatus.InvitationsSent
                    : SignatureStatus.Draft;
                UpdateContractDetails(contractId, ownerUserId, _signatureEngine.TransactionId, _signatureEngine.DocumentId, updateStatus);
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

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetContractAgreement(int contractId, string ownerUserId)
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

                var docResult = await _signatureEngine.GetDocument(DocumentVersion.Draft);
                document = docResult.Item1;
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
        
        private void UpdateContractDetails(int contractId, string ownerUserId, string transactionId, string dpId, SignatureStatus? status)
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

        private List<FormField> PrepareFormFields(Contract contract)
        {
            var fields = new List<FormField>();

            FillHomeOwnerFieilds(fields, contract);
            FillApplicantsFieilds(fields, contract);
            FillEquipmentFieilds(fields, contract);
            FillPaymentFieilds(fields, contract);

            return fields;
        }


        private Tuple<AgreementTemplate, IList<Alert>>  SelectAgreementTemplate(Contract contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            var agreementType = contract.Equipment.AgreementType;
            var equipmentType = contract.Equipment.NewEquipment?.First()?.Type;

            var province =
                contract.PrimaryCustomer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State?.ToProvinceCode();

            var dealerTemplates = _fileRepository.FindAgreementTemplates(at =>
                at.DealerId == contractOwnerId);
            
            // get agreement template 
            AgreementTemplate agreementTemplate = null;

            if (dealerTemplates?.Any() ?? false)                
            {
                // if dealer has templates, select one
                agreementTemplate = dealerTemplates.FirstOrDefault(at =>                
                (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType)
                && (string.IsNullOrEmpty(province) || (at.State?.Contains(province) ?? false))
                && (string.IsNullOrEmpty(equipmentType) || (at.EquipmentType?.Contains(equipmentType) ?? false)));

                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at =>
                        (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType)
                        && (string.IsNullOrEmpty(province) || at.State == province));
                }

                if (agreementTemplate == null && agreementType == AgreementType.RentalApplicationHwt)
                {
                    agreementTemplate = dealerTemplates.FirstOrDefault(at => (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType));
                }
                if (agreementTemplate == null)
                {
                    agreementTemplate = dealerTemplates.First();
                }
            }
            else
            {
                //otherwise select any common template
                var commonTemplates = _fileRepository.FindAgreementTemplates(at => string.IsNullOrEmpty(at.DealerId));
                if (commonTemplates?.Any() ?? false)
                {
                    agreementTemplate = commonTemplates.FirstOrDefault(at => at.AgreementType == contract.Equipment.AgreementType);

                    if (agreementTemplate == null)
                    {
                        agreementTemplate = commonTemplates.FirstOrDefault(at => at.State == province);
                    }

                    if (agreementTemplate == null)
                    {
                        agreementTemplate = commonTemplates.FirstOrDefault(at => at.AgreementForm != null);
                    }
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

        private bool LogoutFromService()
        {
            return _signatureServiceAgent.Logout().GetAwaiter().GetResult();
        }        

        private void FillHomeOwnerFieilds(List<FormField> formFields, Contract contract)
        {
            if (contract.PrimaryCustomer != null)
            {
                formFields.Add(new FormField() {FieldType = FieldType.Text, Name = PdfFormFields.FirstName, Value = contract.PrimaryCustomer.FirstName });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.LastName, Value = contract.PrimaryCustomer.LastName });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.DateOfBirth, Value = contract.PrimaryCustomer.DateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) });
                if (contract.PrimaryCustomer.Locations?.Any() ?? false)
                {
                    var mainAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress);
                    if (mainAddress != null)
                    {
                        formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.InstallationAddress, Value = mainAddress.Street });
                        formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.City, Value = mainAddress.City });
                        formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.Province, Value = mainAddress.State });
                        formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.PostalCode, Value = mainAddress.PostalCode });
                    }
                    var mailAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MailAddress);
                    if (mailAddress != null)
                    {
                        formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsMailingDifferent, Value = "true" });
                        formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.MailingAddress, Value =
                            $"{mailAddress.Street}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}" });                        
                    }
                }
            }

            if (contract.PrimaryCustomer?.Emails?.Any() ?? false)
            {
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.EmailAddress,
                    Value = contract.PrimaryCustomer.Emails.FirstOrDefault(e => e.EmailType == EmailType.Main)?.EmailAddress ?? contract.PrimaryCustomer.Emails.First()?.EmailAddress
                });
            }

            if (contract.PrimaryCustomer?.Phones?.Any() ?? false)
            {
                var homePhone = contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
                var cellPhone = contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell);
                var businessPhone =
                    contract.PrimaryCustomer.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business);

                if (homePhone == null)
                {
                    homePhone = cellPhone;
                    cellPhone = null;
                }
                if (homePhone != null)
                {
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.HomePhone, Value = homePhone.PhoneNum });
                }
                if (cellPhone != null)
                {
                    //formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.HomePhone, Value = homePhone.PhoneNum });
                }
                if (businessPhone != null)
                {
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.BusinessPhone, Value = businessPhone.PhoneNum });
                }
            }
        }

        private void FillApplicantsFieilds(List<FormField> formFields, Contract contract)
        {
            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                var addApplicant = contract.SecondaryCustomers.First();
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.FirstName2, Value = addApplicant.FirstName });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.LastName2, Value = addApplicant.LastName });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.DateOfBirth2, Value = addApplicant.DateOfBirth.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture) });
            }
        }

        private void FillEquipmentFieilds(List<FormField> formFields, Contract contract)
        {
            if (contract.Equipment?.NewEquipment?.Any() ?? false)
            {
                var newEquipments = contract.Equipment.NewEquipment;
                var fstEq = newEquipments.First();

                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.EquipmentQuantity, Value = "1" });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.EquipmentDescription, Value = fstEq.Description.ToString() });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.EquipmentCost, Value = fstEq.Cost.ToString() });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.MonthlyPayment, Value = fstEq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });

                var othersEq = new List<NewEquipment>();
                foreach (var eq in newEquipments)
                {
                    switch (eq.Type)
                    {
                        case "ECO1": // Air Conditioner
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsAirConditioner, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.AirConditionerDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.AirConditionerMonthlyRental, Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });
                            break;
                        case "ECO2": // Boiler
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsBoiler, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.BoilerDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.BoilerMonthlyRental, Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });
                            break;
                        case "ECO3": // Doors
                            othersEq.Add(eq);
                            break;
                        case "ECO4": // Fireplace
                            othersEq.Add(eq);
                            break;
                        case "ECO5": // Furnace
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsFurnace, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.FurnaceDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.FurnaceMonthlyRental, Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });
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
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsWaterFiltration, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.WaterFiltrationDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.WaterFiltrationMonthlyRental, Value = eq.MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });
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
                    formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsOther1, Value = "true" });
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.OtherDetails1, Value = othersEq.First().Description });
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.OtherMonthlyRental1, Value = othersEq.First().MonthlyCost?.ToString("F", CultureInfo.InvariantCulture) });
                }

            }
            if (contract.Equipment != null)
            {
                var paySummary = _contractRepository.GetContractPaymentsSummary(contract.Id);

                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.TotalPayment, Value = paySummary.TotalPayment?.ToString("F", CultureInfo.InvariantCulture) });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.TotalMonthlyPayment, Value = paySummary.MonthlyPayment?.ToString("F", CultureInfo.InvariantCulture) });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.MonthlyPayment, Value = paySummary.MonthlyPayment?.ToString("F", CultureInfo.InvariantCulture) });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.Hst, Value = paySummary.Hst?.ToString("F",CultureInfo.InvariantCulture) });
            }            
        }

        private void FillPaymentFieilds(List<FormField> formFields, Contract contract)
        {
            if (contract.PaymentInfo != null)
            {
                if (contract.PaymentInfo.PaymentType == PaymentType.Enbridge)
                {
                    formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsEnbridge, Value = "true" });
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.EnbridgeAccountNumber, Value = contract.PaymentInfo.EnbridgeGasDistributionAccount });
                    if (string.IsNullOrEmpty(contract.PaymentInfo.EnbridgeGasDistributionAccount))
                    {
                        for (int ch = 1;
                            ch <= Math.Max(contract.PaymentInfo.EnbridgeGasDistributionAccount.Length, 12);
                            ch++)
                        {
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = $"{PdfFormFields.Ean}{ch}", Value = $"{contract.PaymentInfo.EnbridgeGasDistributionAccount[ch-1]}" });
                        }                        
                    }
                }
                else
                {
                    formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsPAD, Value = "true" });
                    formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = contract.PaymentInfo.PrefferedWithdrawalDate == WithdrawalDateType.First ? PdfFormFields.IsPAD1 : PdfFormFields.IsPAD15, Value = "true" });                    
                }                
            }        
        }
    }
}
