using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.SsWeb;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature.EOriginalTypes.Transformation;
using DealnetPortal.Api.Integration.Services.ESignature;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services.Signature
{
    public class EcoreSignatureEngine : ISignatureEngine
    {
        private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;

        private readonly string _eCoreLogin;
        private readonly string _eCorePassword;
        private readonly string _eCoreOrganisation;
        private readonly string _eCoreSignatureRole;
        private readonly string _eCoreAgreementTemplate;
        private readonly string _eCoreCustomerSecurityCode;

        private List<string> _signatureFields = new List<string>() { "Signature1", "Signature2", "Signature3"};
        private List<string> _signatureRoles = new List<string>();        

        public string TransactionId { get; private set; }

        public string DocumentId { get; private set; }

        private Contract _contract { get; set; }

        public EcoreSignatureEngine(IESignatureServiceAgent signatureServiceAgent, IFileRepository fileRepository, ILoggingService loggingService)
        {
            _signatureServiceAgent = signatureServiceAgent;
            _loggingService = loggingService;
            _fileRepository = fileRepository;

            _eCoreLogin = System.Configuration.ConfigurationManager.AppSettings["eCoreUser"];
            _eCorePassword = System.Configuration.ConfigurationManager.AppSettings["eCorePassword"];
            _eCoreOrganisation = System.Configuration.ConfigurationManager.AppSettings["eCoreOrganization"];
            _eCoreSignatureRole = System.Configuration.ConfigurationManager.AppSettings["eCoreSignatureRole"];
            _eCoreAgreementTemplate = System.Configuration.ConfigurationManager.AppSettings["eCoreAgreementTemplate"];
            _eCoreCustomerSecurityCode = System.Configuration.ConfigurationManager.AppSettings["eCoreCustomerSecurityCode"];

            _signatureRoles.Add(_eCoreSignatureRole);
            _signatureRoles.Add($"{_eCoreSignatureRole}2");
            _signatureRoles.Add($"{_eCoreSignatureRole}3");
        }

        public async Task<IList<Alert>> ServiceLogin()
        {
            List<Alert> alerts = new List<Alert>();
            var res = await _signatureServiceAgent.Login(_eCoreLogin, _eCoreOrganisation, _eCorePassword).ConfigureAwait(false);
            alerts.AddRange(res);
            if (alerts.Any(a => a.Type == AlertType.Error))
            {
                _loggingService.LogError("Can't login to eCore signature service with provided credentials");
            }
            return alerts;
        }

        public async Task<IList<Alert>> StartNewTransaction(Contract contract)
        {
            var alerts = new List<Alert>();

            _contract = contract;
            var trRes = await CreateTransaction(_contract);
            if (trRes.Item2?.Any() ?? false)
            {
                alerts.AddRange(trRes.Item2);
            }
            if (trRes.Item2?.All(a => a.Type != AlertType.Error) ?? false)
            {
                var transId = trRes.Item1;
                _loggingService.LogInfo($"eSignature transaction [{transId}] was created successefully");
                TransactionId = transId.ToString();

                var docRes = await CreateAgreementProfile(contract, transId);
                if (docRes.Item2?.Any() ?? false)
                {
                    alerts.AddRange(docRes.Item2);
                }
                if (docRes.Item2?.All(a => a.Type != AlertType.Error) ?? false)
                {
                    var docId = docRes.Item1;
                    _loggingService.LogInfo($"eSignature document profile [{docId}] was created and uploaded successefully");
                    DocumentId = docId.ToString();                    
                }                
            }

            return alerts;
        }

        public async Task<IList<Alert>> InsertDocumentFields(IList<Models.Signature.FormField> formFields)
        {
            var alerts = new List<Alert>();

            var textData = new List<TextData>();
            formFields.ForEach(field =>
            {
                textData.Add(new TextData()
                {
                    Items = new string[] { field.Name },
                    text = field.Value
                });
            });

            long docId;
            if (long.TryParse(DocumentId, out docId))
            {
                var mergeRes = await _signatureServiceAgent.MergeData(docId, textData.ToArray()).ConfigureAwait(false);
                if (mergeRes?.Any() ?? false)
                {
                    alerts.AddRange(mergeRes);
                }
                if (mergeRes?.Any(a => a.Type == AlertType.Error) ?? false)
                {
                    _loggingService.LogError("Can't merge fields with agreement document template in eCore service");
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.EcoreConnectionFailed,
                    Message = "eCore document wasn't created"
                });
            }

            return alerts;
        }

        public async Task<IList<Alert>> InsertSignatures(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();
            long docId;
            if (long.TryParse(DocumentId, out docId))
            {
                var removeRes = await RemoveExtraSignatures(docId, signatureUsers);
                if (removeRes?.Any() ?? false)
                {
                    alerts.AddRange(removeRes);
                }

                // We follow to signatures only if signatureUsers was setted
                if (signatureUsers?.Any() ?? false)
                {

                    var insertRes = await InsertSignatureFields(docId, signatureUsers);
                    if (insertRes?.Any() ?? false)
                    {
                        alerts.AddRange(insertRes);
                    }
                    if (insertRes?.Any(a => a.Type == AlertType.Error) ?? false)
                    {
                        _loggingService.LogWarning(
                            $"Signature fields inserted into agreement document form {docId} with errors");
                    }
                    else
                    {
                        _loggingService.LogInfo(
                            $"Signature fields inserted into agreement document form {docId} successefully");
                    }
                }
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.EcoreConnectionFailed,
                    Message = "eCore document wasn't created"
                });
            }

            return alerts;
        }

        public async Task<IList<Alert>> SendInvitations(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();

            long docId;
            long transId;
            if (long.TryParse(DocumentId, out docId) && long.TryParse(TransactionId, out transId))
            {
                var res =
                    await _signatureServiceAgent.ConfigureSortOrder(transId, new long[] {docId}).ConfigureAwait(false);
                if (res?.Any() ?? false)
                {
                    alerts.AddRange(res);
                }
                if (res?.All(a => a.Type != AlertType.Error) ?? true)
                {
                    var roles = new List<eoConfigureRolesRole>();
                    for (int i = 0; i < Math.Min(signatureUsers.Count, _signatureFields.Count); i++)
                    {
                        roles.Add(
                            new eoConfigureRolesRole()
                            {
                                order = (i + 1).ToString(), //"1",
                                name = _signatureRoles[i], //_eCoreSignatureRole,
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
                    }
                    ;

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
                        for (int i = 0; i < Math.Min(signatureUsers.Count, _signatureFields.Count); i++)
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
            }
            else
            {
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = ErrorConstants.EcoreConnectionFailed,
                    Message = "eCore document wasn't created"
                });
            }

            return alerts;
        }

        private async Task<Tuple<long, IList<Alert>>> CreateTransaction(Contract contract)
        {
            long transId = 0;
            List<Alert> alerts = new List<Alert>();
            var transactionName = contract.PrimaryCustomer?.FirstName + contract.PrimaryCustomer?.LastName;

            var transRes = await _signatureServiceAgent.CreateTransaction(transactionName).ConfigureAwait(false);
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

        private async Task<Tuple<long, IList<Alert>>> CreateAgreementProfile(Contract contract, long transId)
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
                var resPr = await _signatureServiceAgent.CreateDocumentProfile(transId, _eCoreAgreementTemplate, null).ConfigureAwait(false);
                if (resPr.Item2?.Any() ?? false)
                {
                    alerts.AddRange(resPr.Item2);
                }
                if (resPr.Item2?.All(a => a.Type != AlertType.Error) ?? true)
                {
                    dpId = resPr.Item1.sid;

                    var resDv = await _signatureServiceAgent.UploadDocument(dpId, agreementTemplate.AgreementForm, agreementTemplate.TemplateName).ConfigureAwait(false);
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

        private async Task<IList<Alert>> RemoveExtraSignatures(long docId, IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();

            //document can have more signature fields then signature users
            var signToRemove = (signatureUsers == null || !signatureUsers.Any())
                ? _signatureFields.Count
                : _signatureFields.Count - signatureUsers.Count;

            if (signToRemove > 0)
            {
                _loggingService.LogInfo($"{signToRemove} signature fields will be removed");

                var formFields = new List<ServiceAgents.ESignature.EOriginalTypes.Transformation.FormField>();

                for (var i = (_signatureFields.Count - signToRemove); i < _signatureFields.Count; i++)
                {
                    formFields.Add(new ServiceAgents.ESignature.EOriginalTypes.Transformation.FormField()
                    {
                        Item = _signatureFields[i]
                    });
                }

                var removeRes = await _signatureServiceAgent.RemoveFormFields(docId, formFields.ToArray()).ConfigureAwait(false);
                if (removeRes?.Item2?.Any() ?? false)
                {
                    alerts.AddRange(removeRes.Item2);
                }
            }

            return alerts;
        }

        private async Task<IList<Alert>> InsertSignatureFields(long docId, IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();

            // for now we accept only 1st customer

            var formFields = new List<ServiceAgents.ESignature.EOriginalTypes.Transformation.FormField>();
            // lets insert one by one
            for (int i = 0; i < Math.Min(signatureUsers.Count, _signatureFields.Count); i++)
            {
                formFields.Add(
                    new ServiceAgents.ESignature.EOriginalTypes.Transformation.FormField()
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
                    });
                var resDv = await _signatureServiceAgent.EditFormFields(docId, formFields.ToArray()).ConfigureAwait(false);
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
    }
}
