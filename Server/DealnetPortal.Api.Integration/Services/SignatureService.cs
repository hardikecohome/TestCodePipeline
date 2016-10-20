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

                var logRes = await _signatureEngine.ServiceLogin();// LoginToService();
                if (logRes.Any(a => a.Type == AlertType.Error))
                {
                    LogAlerts(alerts);
                    return alerts;
                }

                var trRes = await _signatureEngine.StartNewTransaction(contract);                

                //TODO: !!!
                //var docId = docRes.Item1;
                //_loggingService.LogInfo($"eSignature document profile [{docId}] was created and uploaded successefully");
                //UpdateContractDetails(contractId, ownerUserId, transId.ToString(), docId.ToString(), SignatureStatus.ProfileCreated);

                var insertRes = InsertAgreementFields(docId, fields);
                if (insertRes?.Any() ?? false)
                {
                    alerts.AddRange(insertRes);
                }
                if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                {
                    _loggingService.LogWarning($"Fields merged with agreement document {docId} with errors");
                    //LogAlerts(alerts);
                    //return alerts;
                }
                else
                {
                    _loggingService.LogInfo($"Fields merged with agreement document form {docId} successefully");
                    UpdateContractDetails(contractId, ownerUserId, null, null, SignatureStatus.FieldsMerged);
                }                

                var removeRes = RemoveExtraSignatures(docId, signatureUsers);
                if (removeRes?.Any() ?? false)
                {
                    alerts.AddRange(removeRes);
                }

                // We follow to signatures only if signatureUsers was setted
                if (signatureUsers?.Any() ?? false)
                {
                    insertRes = InsertSignatureFields(docId, signatureUsers);
                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        _loggingService.LogWarning($"Signature fields inserted into agreement document form {docId} with errors");
                        //LogAlerts(alerts);
                        //return alerts;
                    }
                    else
                    {
                        _loggingService.LogInfo(
                            $"Signature fields inserted into agreement document form {docId} successefully");
                    }

                    insertRes = SendInvitations(transId, docId, signatureUsers);
                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        LogAlerts(alerts);
                        return alerts;
                    }
                    _loggingService.LogInfo($"Invitations for agreement document form {docId} sent successefully");
                    UpdateContractDetails(contractId, ownerUserId, null, null, SignatureStatus.InvitationsSent);
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
            return alerts;
        }

        public IList<Alert> GetContractAgreement(int contractId, string ownerUserId)
        {
            List<Alert> alerts = new List<Alert>();

            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                if (!string.IsNullOrEmpty(contract.Details.SignatureDocumentId) ||
                    !contract.Details.SignatureStatus.HasValue)
                {
                    var logRes = LoginToService();
                    if (logRes.Any(a => a.Type == AlertType.Error))
                    {
                        LogAlerts(alerts);
                        return alerts;
                    }
                    long docId = 0;
                    long.TryParse(contract.Details.SignatureDocumentId, out docId);
                    var getRes = _signatureServiceAgent.GetCopy(docId).GetAwaiter().GetResult();
                    if (getRes?.Item2?.Any() ?? false)
                    {
                        alerts.AddRange(getRes.Item2);
                    }
                    //TODO: add return of document

                }
                else
                {
                    alerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Message = $"Digital signature for contract with id {contractId} doesn't complete",
                        Header = "Can't get contract agreement"
                    });
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Message = $"Can't get contract with id {contractId}",
                    Header = "Can't get contract"
                });
            }

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

        private Dictionary<string, string> PrepareFormFields(Contract contract)
        {
            var fields = new Dictionary<string, string>();

            FillHomeOwnerFieilds(fields, contract);
            FillApplicantsFieilds(fields, contract);
            FillEquipmentFieilds(fields, contract);
            FillPaymentFieilds(fields, contract);

            return fields;
        }        

        private bool LogoutFromService()
        {
            return _signatureServiceAgent.Logout().GetAwaiter().GetResult();
        }

        private Tuple<long, IList<Alert>> CreateTransaction(Contract contract)
        {
            long transId = 0;
            List<Alert> alerts = new List<Alert>();
            var transactionName = contract.PrimaryCustomer?.FirstName + contract.PrimaryCustomer?.LastName;

            var transRes = _signatureServiceAgent.CreateTransaction(transactionName).GetAwaiter().GetResult();
            alerts.AddRange(transRes.Item2);
            if (transRes.Item2.All(a => a.Type != AlertType.Error))
            {
                transId = transRes.Item1.sid;
            }
            else
            {
                _loggingService.LogError("Can't create eCore transaction");
            }

            return new Tuple<long, IList<Alert>>(transId, alerts);
        }

        private Tuple<long, IList<Alert>> CreateAgreementProfile(Contract contract, long transId)        
        {
            long dpId = 0;
            List<Alert> alerts = new List<Alert>();
            var agreementType = contract.Equipment.AgreementType;
            var province =
                contract.PrimaryCustomer.Locations.FirstOrDefault(l => l.AddressType == AddressType.MainAddress)?.State?.ToProvinceCode();
            // get agreement template
            var agreementTemplate = _fileRepository.FindAgreementTemplate(at => 
                at.AgreementType == contract.Equipment.AgreementType && (string.IsNullOrEmpty(province) || at.State == province));
            if (agreementTemplate == null && agreementType == AgreementType.RentalApplicationHwt)
            {
                _fileRepository.FindAgreementTemplate(at =>
                    at.AgreementType == AgreementType.RentalApplication && (string.IsNullOrEmpty(province) || at.State == province));
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

            if (agreementTemplate?.AgreementForm != null)
            {
                var resPr = _signatureServiceAgent.CreateDocumentProfile(transId, _eCoreAgreementTemplate, null).GetAwaiter().GetResult();
                if (resPr.Item2?.Any() ?? false)
                {
                    alerts.AddRange(resPr.Item2);
                }
                if (resPr.Item2?.All(a => a.Type != AlertType.Error) ?? true)
                {
                    dpId = resPr.Item1.sid;

                    var resDv = _signatureServiceAgent.UploadDocument(dpId, agreementTemplate.AgreementForm, agreementTemplate.TemplateName).GetAwaiter().GetResult();
                    if (resDv.Item2?.Any() ?? false)
                    {
                        alerts.AddRange(resDv.Item2);
                    }
                    if (resDv.Item2?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        _loggingService.LogError("Can't upload agreement template to eCore service");
                    }
                }
                else
                {
                    _loggingService.LogError("Can't create eCore document profile");
                }                
            }
            else
            {
                var errorMsg =
                    $"Can't find agreement template for contract with province = {province} and type = {agreementType}";
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "Can't find agreement template",
                    Message = errorMsg
                });
            }

            return new Tuple<long, IList<Alert>>(dpId, alerts);
        }

        private IList<Alert> InsertAgreementFields(long docId, Dictionary<string, string> formFields)
        {
            var alerts = new List<Alert>();

            var textData = new List<TextData>();
            formFields.ForEach(field =>
            {
                textData.Add(new TextData()
                {
                    Items = new string[] { field.Key },
                    text = field.Value
                });
            });

            var mergeRes = _signatureServiceAgent.MergeData(docId, textData.ToArray()).GetAwaiter().GetResult();
            if (mergeRes?.Any() ?? false)
            {
                alerts.AddRange(mergeRes);
            }
            if (mergeRes?.Any(a => a.Type == AlertType.Error) ?? false)
            {
                _loggingService.LogError("Can't merge fields with agreement document template in eCore service");
            }

            return alerts;
        }

        private IList<Alert> RemoveExtraSignatures(long docId, SignatureUser[] signatureUsers)
        {
            var alerts = new List<Alert>();

            //document can have more signature fields then signature users
            var signToRemove = (signatureUsers == null || !signatureUsers.Any())
                ? _signatureFields.Count
                : _signatureFields.Count - signatureUsers.Length;

            if (signToRemove > 0)
            {
                _loggingService.LogInfo($"{signToRemove} signature fields will be removed");

                var formFields = new List<FormField>();

                for (var i = (_signatureFields.Count - signToRemove); i < _signatureFields.Count; i++)
                {
                    formFields.Add(new FormField()
                    {
                        Item = _signatureFields[i]
                    });
                }

                var removeRes = _signatureServiceAgent.RemoveFormFields(docId, formFields.ToArray()).GetAwaiter().GetResult();
                if (removeRes?.Item2?.Any() ?? false)
                {
                    alerts.AddRange(removeRes.Item2);
                }                
            }                                             

            return alerts;
        }

        private IList<Alert> InsertSignatureFields(long docId, SignatureUser[] signatureUsers)
        {
            var alerts = new List<Alert>();

            // for now we accept only 1st customer

            var formFields = new List<FormField>();
            // lets insert one by one
            for (int i = 0; i < Math.Min(signatureUsers.Length, _signatureFields.Count); i++)
            {
                formFields = new List<FormField>()
                { 
                    new FormField()
                    {
                        Item = _signatureFields[i],//"Signature1",
                        customProperty = new List<CustomProperty>()
                        {
                            new CustomProperty()
                            {
                                name = "role",
                                Value = _signatureRoles[i]//_eCoreSignatureRole
                            },
                            new CustomProperty()
                            {
                                name = "label",
                                Value = _signatureFields[i]
                            },
                            new CustomProperty()
                            {
                                name = "type",
                                Value = "signature"
                            },
                            new CustomProperty()
                            {
                                name = "required",
                                Value = "true"
                            },
                            new CustomProperty()
                            {
                                name = "initialValueType",
                                Value = "fullName"
                            },
                            //new CustomProperty()
                            //{
                            //    name = "protectedField",
                            //    Value = "false"
                            //},
                            new CustomProperty()
                            {
                                name = "displayOrder",
                                Value = (i+1).ToString()//"1"
                            }                    
                        }
                    }
                };
                var resDv = _signatureServiceAgent.EditFormFields(docId, formFields.ToArray()).GetAwaiter().GetResult();
                if (resDv?.Item2?.Any() ?? false)
                {
                    alerts.AddRange(resDv.Item2);
                }
            }            
            
            if (alerts.Any(a => a.Type == AlertType.Error))
            {
                _loggingService.LogError("Can't edit signature fields in agreement document template in eCore service");
            }

            return alerts;
        }

        private IList<Alert> SendInvitations(long transId, long docId, SignatureUser[] signatureUsers)
        {
            var alerts = new List<Alert>();

            var res = _signatureServiceAgent.ConfigureSortOrder(transId, new long[] { docId }).GetAwaiter().GetResult();
            if (res?.Any() ?? false)
            {
                alerts.AddRange(res);
            }
            if (res?.All(a => a.Type != AlertType.Error) ?? true)
            {
                var roles = new List<eoConfigureRolesRole>();
                for (int i = 0; i < Math.Min(signatureUsers.Length, _signatureFields.Count); i++)
                {
                    roles.Add(
                        new eoConfigureRolesRole()
                        {
                            order = (i+1).ToString(),//"1",
                            name = _signatureRoles[i],//_eCoreSignatureRole,
                            firstName = signatureUsers[i].FirstName ?? "Fst",
                            lastName = signatureUsers[i].LastName ?? "name",
                            eMail = signatureUsers[i].EmailAddress,
                            ItemsElementName = new ItemsChoiceType[] {ItemsChoiceType.securityCode},
                            Items = new string[] {_eCoreCustomerSecurityCode},
                            required = true,
                            signatureCaptureMethod = new eoConfigureRolesRoleSignatureCaptureMethod()
                            {
                                Value = signatureCaptureMethodType.TYPE
                            },
                        });
                };

                res = _signatureServiceAgent.ConfigureRoles(transId, roles.ToArray()).GetAwaiter().GetResult();
                if (res?.Any() ?? false)
                {
                    alerts.AddRange(res);
                }
                if (res?.All(a => a.Type != AlertType.Error) ?? true)
                {
                    //res =
                    //        _signatureServiceAgent.ConfigureInvitation(transId, _eCoreSignatureRole,
                    //            signatureUsers.First().FirstName, signatureUsers.First().LastName,
                    //            signatureUsers.First().EmailAddress).GetAwaiter().GetResult();
                    for (int i = 0; i < Math.Min(signatureUsers.Length, _signatureFields.Count); i++)
                    {
                        res =
                            _signatureServiceAgent.ConfigureInvitation(transId, _signatureRoles[i],
                                signatureUsers[i].FirstName ?? "fst", signatureUsers[i].LastName ?? "name",
                                signatureUsers[i].EmailAddress).GetAwaiter().GetResult();
                        if (res?.Any() ?? false)
                        {
                            alerts.AddRange(res);
                        }
                    }
                }                
            }

            if (alerts.Any(a => a.Type == AlertType.Error))
            {
                _loggingService.LogError("Can't send invitations to users");
            }

            return alerts;
        }

        private void FillHomeOwnerFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.PrimaryCustomer != null)
            {
                formFields[PdfFormFields.FirstName] = contract.PrimaryCustomer.FirstName;
                formFields[PdfFormFields.LastName] = contract.PrimaryCustomer.LastName;
                formFields[PdfFormFields.DateOfBirth] = contract.PrimaryCustomer.DateOfBirth.ToShortDateString();
                if (contract.PrimaryCustomer.Locations?.Any() ?? false)
                {
                    var mainAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MainAddress);
                    if (mainAddress != null)
                    {
                        formFields[PdfFormFields.InstallationAddress] = mainAddress.Street;
                        formFields[PdfFormFields.City] = mainAddress.City;
                        formFields[PdfFormFields.Province] = mainAddress.State;
                        formFields[PdfFormFields.PostalCode] = mainAddress.PostalCode;
                    }
                    var mailAddress =
                        contract.PrimaryCustomer?.Locations?.FirstOrDefault(
                            l => l.AddressType == AddressType.MailAddress);
                    if (mailAddress != null)
                    {
                        formFields[PdfFormFields.IsMailingDifferent] = "true";
                        formFields[PdfFormFields.MailingAddress] =
                            $"{mailAddress.Street}, {mailAddress.City}, {mailAddress.State}, {mailAddress.PostalCode}";
                    }
                }
            }

            //TODO: re-implement after refactoring
            //if (contract.ContactInfo != null)
            //{
            //    if (!string.IsNullOrEmpty(contract.ContactInfo.EmailAddress))
            //    {
            //        formFields[PdfFormFields.EmailAddress] = contract.ContactInfo.EmailAddress;
            //    }
            //    if (contract.ContactInfo.Phones?.Any() ?? false)
            //    {
            //        var homePhone = contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Home);
            //        var cellPhone = contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Cell);
            //        var businessPhone =
            //            contract.ContactInfo.Phones.FirstOrDefault(p => p.PhoneType == PhoneType.Business);

            //        if (homePhone == null)
            //        {
            //            homePhone = cellPhone;
            //            cellPhone = null;
            //        }
            //        if (homePhone != null)
            //        {
            //            formFields[PdfFormFields.HomePhone] = homePhone.PhoneNum;
            //        }
            //        if (cellPhone != null)
            //        {
            //            //formFields[PdfFormFields.Ce] = cellPhone.PhoneNum;
            //        }
            //        if (businessPhone != null)
            //        {
            //            formFields[PdfFormFields.BusinessPhone] = businessPhone.PhoneNum;
            //        }
            //    }
            //}
        }

        private void FillApplicantsFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.SecondaryCustomers?.Any() ?? false)
            {
                var addApplicant = contract.SecondaryCustomers.First();
                formFields[PdfFormFields.FirstName2] = addApplicant.FirstName;
                formFields[PdfFormFields.LastName2] = addApplicant.LastName;
                formFields[PdfFormFields.DateOfBirth2] = addApplicant.DateOfBirth.ToShortDateString();
            }
        }

        private void FillEquipmentFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.Equipment?.NewEquipment?.Any() ?? false)
            {
                var newEquipments = contract.Equipment.NewEquipment;
                var fstEq = newEquipments.First();

                formFields[PdfFormFields.EquipmentQuantity] = fstEq.Quantity.ToString();
                formFields[PdfFormFields.EquipmentDescription] = fstEq.Description.ToString();
                formFields[PdfFormFields.EquipmentCost] = fstEq.Cost.ToString(CultureInfo.CurrentCulture);
                formFields[PdfFormFields.MonthlyPayment] = fstEq.MonthlyCost.ToString(CultureInfo.CurrentCulture);

                var othersEq = new List<NewEquipment>();
                foreach (var eq in newEquipments)
                {
                    switch (eq.Type)
                    {
                        case "ECO1": // Air Conditioner
                            formFields[PdfFormFields.IsAirConditioner] = "true";
                            formFields[PdfFormFields.AirConditionerDetails] = eq.Description;
                            formFields[PdfFormFields.AirConditionerMonthlyRental] = eq.MonthlyCost.ToString(CultureInfo.CurrentCulture);
                            break;
                        case "ECO2": // Boiler
                            formFields[PdfFormFields.IsBoiler] = "true";
                            formFields[PdfFormFields.BoilerDetails] = eq.Description;
                            formFields[PdfFormFields.BoilerMonthlyRental] = eq.MonthlyCost.ToString(CultureInfo.CurrentCulture);
                            break;
                        case "ECO3": // Doors
                            othersEq.Add(eq);
                            break;
                        case "ECO4": // Fireplace
                            othersEq.Add(eq);
                            break;
                        case "ECO5": // Furnace
                            formFields[PdfFormFields.IsFurnace] = "true";
                            formFields[PdfFormFields.FurnaceDetails] = eq.Description;
                            formFields[PdfFormFields.FurnaceMonthlyRental] = eq.MonthlyCost.ToString(CultureInfo.CurrentCulture);
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
                            formFields[PdfFormFields.IsWaterFiltration] = "true";
                            formFields[PdfFormFields.WaterFiltrationDetails] = eq.Description;
                            formFields[PdfFormFields.WaterFiltrationMonthlyRental] = eq.MonthlyCost.ToString(CultureInfo.CurrentCulture);
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
                    formFields[PdfFormFields.IsOther1] = "true";
                    formFields[PdfFormFields.OtherDetails1] = othersEq.First().Description;
                    formFields[PdfFormFields.OtherMonthlyRental1] = othersEq.First().MonthlyCost.ToString(CultureInfo.CurrentCulture);
                }

            }
            if (contract.Equipment != null)
            {
                formFields[PdfFormFields.TotalPayment] =
                    contract.Equipment.TotalMonthlyPayment.ToString(CultureInfo.CurrentCulture);
                formFields[PdfFormFields.TotalMonthlyPayment] =
                    contract.Equipment.TotalMonthlyPayment.ToString(CultureInfo.CurrentCulture);
            }            
        }

        private void FillPaymentFieilds(Dictionary<string, string> formFields, Contract contract)
        {
            if (contract.PaymentInfo != null)
            {
                if (contract.PaymentInfo.PaymentType == PaymentType.Enbridge)
                {
                    formFields[PdfFormFields.IsEnbridge] = "true";
                    formFields[PdfFormFields.EnbridgeAccountNumber] = contract.PaymentInfo.EnbridgeGasDistributionAccount;
                }
                else
                {
                    formFields[PdfFormFields.IsPAD] = "true";
                    formFields[contract.PaymentInfo.PrefferedWithdrawalDate == WithdrawalDateType.First ? PdfFormFields.IsPAD1 : PdfFormFields.IsPAD15] = "true";                    
                }                
            }        
        }
    }
}
