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

                var logRes = await _signatureEngine.ServiceLogin().ConfigureAwait(false);// LoginToService();
                if (logRes.Any(a => a.Type == AlertType.Error))
                {
                    LogAlerts(alerts);
                    return alerts;
                }

                var agreementTemplate = SelectAgreementTemplate(contract, ownerUserId);
                if (agreementTemplate == null)
                {
                    LogAlerts(alerts);
                    return alerts;
                }

                var trRes = await _signatureEngine.StartNewTransaction(contract, agreementTemplate);

                if (trRes?.Any() ?? false)
                {
                    alerts.AddRange(trRes);
                }
                //TODO: !!!
                //var docId = docRes.Item1;
                //_loggingService.LogInfo($"eSignature document profile [{docId}] was created and uploaded successefully");
                //UpdateContractDetails(contractId, ownerUserId, transId.ToString(), docId.ToString(), SignatureStatus.ProfileCreated);

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

                insertRes = await _signatureEngine.SendInvitations(signatureUsers);
                    
                    //SendInvitations(transId, docId, signatureUsers);
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
                UpdateContractDetails(contractId, ownerUserId, _signatureEngine.TransactionId, _signatureEngine.DocumentId, SignatureStatus.InvitationsSent);
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

        public IList<Alert> GetContractAgreement(int contractId, string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();

            //var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            //if (contract != null)
            //{
            //    if (!string.IsNullOrEmpty(contract.Details.SignatureDocumentId) ||
            //        !contract.Details.SignatureStatus.HasValue)
            //    {
            //        var logRes = LoginToService();
            //        if (logRes.Any(a => a.Type == AlertType.Error))
            //        {
            //            LogAlerts(alerts);
            //            return alerts;
            //        }
            //        long docId = 0;
            //        long.TryParse(contract.Details.SignatureDocumentId, out docId);
            //        var getRes = _signatureServiceAgent.GetCopy(docId).GetAwaiter().GetResult();
            //        if (getRes?.Item2?.Any() ?? false)
            //        {
            //            alerts.AddRange(getRes.Item2);
            //        }
            //        //TODO: add return of document

            //    }
            //    else
            //    {
            //        alerts.Add(new Alert()
            //        {
            //            Type = AlertType.Error,
            //            Message = $"Digital signature for contract with id {contractId} doesn't complete",
            //            Header = "Can't get contract agreement"
            //        });
            //    }
            //}
            //else
            //{
            //    alerts.Add(new Alert()
            //    {
            //        Type = AlertType.Error,
            //        Message = $"Can't get contract with id {contractId}",
            //        Header = "Can't get contract"
            //    });
            //}

            return alerts;
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


        private AgreementTemplate SelectAgreementTemplate(Contract contract, string contractOwnerId)
        {
            var agreementType = contract.Equipment.AgreementType;
            var province =
                contract.PrimaryCustomer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State?.ToProvinceCode();
            // get agreement template            

            var agreementTemplate = _fileRepository.FindAgreementTemplate(at =>
                at.DealerId == contractOwnerId
                && (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType)
                && (string.IsNullOrEmpty(province) || at.State == province));

            if (agreementTemplate == null && agreementType == AgreementType.RentalApplicationHwt)
            {
                agreementTemplate = _fileRepository.FindAgreementTemplate(at =>
                at.DealerId == contractOwnerId
                && (!at.AgreementType.HasValue || at.AgreementType == contract.Equipment.AgreementType));
            }

            if (agreementTemplate == null)
            {
                agreementTemplate = _fileRepository.FindAgreementTemplate(at => at.DealerId == contractOwnerId);
            }

            if (agreementTemplate == null)
            {
                agreementTemplate = _fileRepository.FindAgreementTemplate(at => at.AgreementType == contract.Equipment.AgreementType);
            }

            if (agreementTemplate == null)
            {
                agreementTemplate = _fileRepository.FindAgreementTemplate(at => at.State == province);
            }

            if (agreementTemplate == null)
            {
                agreementTemplate = _fileRepository.FindAgreementTemplate(at => at.AgreementForm != null);
            }

            if (agreementTemplate == null)
            {
                var errorMsg =
                    $"Can't find agreement template for a dealer contract {contractOwnerId} with province = {province} and type = {agreementType}";
                //alerts.Add(new Alert()
                //{
                //    Type = AlertType.Error,
                //    Header = "Can't find agreement template",
                //    Message = errorMsg
                //});
            }

            return agreementTemplate;
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
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.DateOfBirth, Value = contract.PrimaryCustomer.DateOfBirth.ToShortDateString() });
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
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.DateOfBirth2, Value = addApplicant.DateOfBirth.ToShortDateString() });
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
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.MonthlyPayment, Value = fstEq.MonthlyCost?.ToString(CultureInfo.CurrentCulture) });

                var othersEq = new List<NewEquipment>();
                foreach (var eq in newEquipments)
                {
                    switch (eq.Type)
                    {
                        case "ECO1": // Air Conditioner
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsAirConditioner, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.AirConditionerDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.AirConditionerMonthlyRental, Value = eq.MonthlyCost?.ToString(CultureInfo.CurrentCulture) });
                            break;
                        case "ECO2": // Boiler
                            formFields.Add(new FormField() { FieldType = FieldType.CheckBox, Name = PdfFormFields.IsBoiler, Value = "true" });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.BoilerDetails, Value = eq.Description });
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.BoilerMonthlyRental, Value = eq.MonthlyCost?.ToString(CultureInfo.CurrentCulture) });
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
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.FurnaceMonthlyRental, Value = eq.MonthlyCost?.ToString(CultureInfo.CurrentCulture) });
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
                            formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.WaterFiltrationMonthlyRental, Value = eq.MonthlyCost?.ToString(CultureInfo.CurrentCulture) });
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
                    formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.OtherMonthlyRental1, Value = othersEq.First().MonthlyCost?.ToString(CultureInfo.CurrentCulture) });
                }

            }
            if (contract.Equipment != null)
            {
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.TotalPayment, Value = contract.Equipment.TotalMonthlyPayment?.ToString(CultureInfo.CurrentCulture) });
                formFields.Add(new FormField() { FieldType = FieldType.Text, Name = PdfFormFields.TotalMonthlyPayment, Value = contract.Equipment.TotalMonthlyPayment?.ToString(CultureInfo.CurrentCulture) });                
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
