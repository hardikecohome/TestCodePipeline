using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.Api.Integration.Services.Signature
{
    public class DocuSignSignatureEngine : ISignatureEngine
    {
        private readonly string _baseUrl;
        private readonly string _dsUser;
        private readonly string _dsPassword;
        private readonly string _dsIntegratorKey;

        private ILoggingService _loggingService;

        public string AccountId { get; private set; }        

        public string TransactionId { get; set; }

        public string DocumentId { get; set; }

        private Document _document { get; set; }

        private string _templateId { get; set; }

        private bool _templateUsed { get; set; }

        private List<Text> _textTabs { get; set; }
        private List<Checkbox> _checkboxTabs { get; set; }
        private List<SignHere> _signHereTabs { get; set; }
        private List<Signer> _signers { get; set; }
        private List<CarbonCopy> _copyViewers { get; set; }
        private EnvelopeDefinition _envelopeDefinition { get; set; }

        private const string EmailSubject = "Please Sign Agreement";

        public DocuSignSignatureEngine(ILoggingService loggingService)
        {
            _loggingService = loggingService;

            _baseUrl = System.Configuration.ConfigurationManager.AppSettings["DocuSignApiUrl"];
            _dsUser = System.Configuration.ConfigurationManager.AppSettings["DocuSignUser"];
            _dsPassword = System.Configuration.ConfigurationManager.AppSettings["DocuSignPassword"];
            _dsIntegratorKey = System.Configuration.ConfigurationManager.AppSettings["DocuSignIntegratorKey"];
        }

        public async Task<IList<Alert>> ServiceLogin()
        {
            List<Alert> alerts = new List<Alert>();

            var loginAlerts = await Task.Run(() =>
            {
                var logAlerts = new List<Alert>();
                ApiClient apiClient = new ApiClient(_baseUrl);
                string authHeader = CreateAuthHeader(_dsUser, _dsPassword, _dsIntegratorKey);
                Configuration.Default.ApiClient = apiClient;

                if (Configuration.Default.DefaultHeader.ContainsKey("X-DocuSign-Authentication"))
                {
                    Configuration.Default.DefaultHeader.Remove("X-DocuSign-Authentication");
                }
                Configuration.Default.AddDefaultHeader("X-DocuSign-Authentication", authHeader);

                AuthenticationApi authApi = new AuthenticationApi();

                AuthenticationApi.LoginOptions options = new AuthenticationApi.LoginOptions();
                options.apiPassword = "true";
                options.includeAccountIdGuid = "true";
                LoginInformation loginInfo = authApi.Login(options);

                // find the default account for this user
                foreach (LoginAccount loginAcct in loginInfo.LoginAccounts)
                {
                    if (loginAcct.IsDefault == "true")
                    {
                        AccountId = loginAcct.AccountId;
                        break;
                    }
                }

                if (string.IsNullOrEmpty(AccountId))
                {
                    _loggingService.LogError("Can't login to DocuSign service");
                    logAlerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = "DocuSign error",
                        Message = "Can't login to DocuSign service"
                    });
                }

                return logAlerts;
            }).ConfigureAwait(false);

            if (loginAlerts.Any())
            {
                alerts.AddRange(loginAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> InitiateTransaction(Contract contract, AgreementTemplate agreementTemplate)
        {
            var alerts = new List<Alert>();

            await Task.Run(() =>
            {                
                if (contract != null & agreementTemplate != null)
                {
                    if (!string.IsNullOrEmpty(agreementTemplate.ExternalTemplateId))
                    {
                        _templateId = agreementTemplate.ExternalTemplateId;
                        _templateUsed = true;
                    }
                    else
                    {
                        _templateUsed = false;
                        _document = new Document
                        {
                            DocumentBase64 = System.Convert.ToBase64String(agreementTemplate.AgreementForm),
                            Name = agreementTemplate.TemplateName,
                            DocumentId = contract.Id.ToString(),
                            TransformPdfFields = "true"
                        };
                    }
                    _signers = new List<Signer>();
                    _copyViewers = new List<CarbonCopy>();
                    _envelopeDefinition = new EnvelopeDefinition();
                }
            });                      

            return alerts;
        }        

        public async Task<IList<Alert>> InsertDocumentFields(IList<FormField> formFields)
        {
            var alerts = new List<Alert>();
            _textTabs = new List<Text>();
            _checkboxTabs = new List<Checkbox>();

            await Task.Run(() =>
            {
                if (formFields?.Any() ?? false)
                {
                    formFields.ForEach(ff =>
                    {
                        if (ff.FieldType == FieldType.CheckBox)
                        {
                            _checkboxTabs.Add(new Checkbox()
                            {
                                TabLabel = ff.Name,
                                Selected = ff.Value
                            });
                        }
                        else //Text
                        {
                            _textTabs.Add(new Text()
                            {
                                TabLabel = ff.Name,
                                Value = ff.Value
                            });
                        }
                    });
                }
            });

            return alerts;
        }

        public async Task<IList<Alert>> InsertSignatures(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();
            _signHereTabs = new List<SignHere>();

            await Task.Run(() =>
            {
                if (signatureUsers?.Any() ?? false)
                {
                    int signN = 1;
                    _signers?.Clear();
                    signatureUsers.Where(s => s.Role == SignatureRole.Signer).ForEach(s =>
                    {
                        _signers.Add(CreateSigner(s, signN++));
                    });
                    signatureUsers.Where(s => s.Role == SignatureRole.AdditionalApplicant).ForEach(s =>
                    {
                        _signers.Add(CreateSigner(s, signN++));
                    });
                    _copyViewers?.Clear();
                    signatureUsers.Where(s => s.Role == SignatureRole.CopyViewer).ForEach(s =>
                    {
                        _copyViewers.Add(new CarbonCopy()
                        {
                            Email = s.EmailAddress,
                            Name = $"{s.FirstName} {s.LastName}",
                            RoutingOrder = signN.ToString(),
                            RecipientId = signN.ToString(),                            
                        });
                        signN++;
                    });
                }

            });

            return alerts;
        }        

        public async Task<IList<Alert>> SendInvitations(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();
            
            EnvelopesApi envelopesApi = new EnvelopesApi();
            bool recreateEnvelope = false;
            bool recreateRecipients = false;

            if (!string.IsNullOrEmpty(TransactionId))
            {
                var envelope = await envelopesApi.GetEnvelopeAsync(AccountId, TransactionId);
                if (envelope != null)
                {
                    var reciepents = await envelopesApi.ListRecipientsAsync(AccountId, TransactionId);
                    await InsertSignatures(signatureUsers);
                    recreateRecipients = !AreRecipientsEqual(reciepents);
                    if (envelope.Status == "sent" && recreateRecipients)
                    {
                        recreateEnvelope = true;
                    }
                }
                else
                {
                    recreateEnvelope = true;
                }
                if (!recreateEnvelope)
                {
                    if (recreateRecipients)
                    {
                        var recipients = new Recipients()
                        {
                            Signers = _signers,
                            CarbonCopies = _copyViewers
                        };
                        var updateRes = await envelopesApi.UpdateRecipientsAsync(AccountId, TransactionId, recipients);
                    }
                    envelope.Status = "sent";
                    var updateEnvelopeRes = await envelopesApi.UpdateAsync(AccountId, TransactionId, envelope);
                }
            }
            else
            {
                recreateEnvelope = true;
            }

            if (recreateEnvelope)
            {
                _envelopeDefinition = PrepareEnvelope();
                var envAlerts = CreateEnvelope(_envelopeDefinition);
                if (envAlerts.Any())
                {
                    alerts.AddRange(envAlerts);
                }
            }            

            //_envelopeDefinition.EmailSubject = EmailSubject;
            //_envelopeDefinition.CompositeTemplates = new List<CompositeTemplate>()
            //{
            //    new CompositeTemplate()
            //    {
            //        InlineTemplates = new List<InlineTemplate>()
            //        {
            //            new InlineTemplate()
            //            {
            //                Sequence = "1",
            //                Recipients = new Recipients()
            //                {
            //                    Signers = _signers,
            //                    CarbonCopies = _copyViewers
            //                }
            //            }
            //        },
            //        Document = _document
            //    },
            //};
            //_envelopeDefinition.Status = "sent";                

            //EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(AccountId, _envelopeDefinition);

            //if (!string.IsNullOrEmpty(envelopeSummary?.EnvelopeId))
            //{
            //    EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountId, envelopeSummary.EnvelopeId);

            //    TransactionId = envelopeSummary.EnvelopeId;
            //    _loggingService.LogInfo($"DocuSign document was successfully created: envelope {TransactionId}, document {docsList?.EnvelopeDocuments?.FirstOrDefault()?.DocumentId}");
            //    DocumentId = docsList?.EnvelopeDocuments?.FirstOrDefault()?.DocumentId;
            //}
            //else
            //{
            //    _loggingService.LogError("DocuSign document wasn't created");
            //    sendAlerts.Add(new Alert()
            //    {
            //        Type = AlertType.Error,
            //        Header = "DocuSign error",
            //        Message = "DocuSign document wasn't created"
            //    });

            //}                           

            return alerts;
        }

        public async Task<IList<Alert>> CreateDraftDocument(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();

            var tskAlerts = await Task.Run(() =>
            {                
                _envelopeDefinition = PrepareEnvelope();
                _envelopeDefinition.Status = "created";

                var sendAlerts = CreateEnvelope(_envelopeDefinition);                

                return sendAlerts;
            }).ConfigureAwait(false);

            if (tskAlerts.Any())
            {
                alerts.AddRange(tskAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> GetDocument(DocumentVersion documentVersion)
        {
            var alerts = new List<Alert>();

            var tskAlerts = await Task.Run(() =>
            {
                var sendAlerts = new List<Alert>();

                if (!string.IsNullOrEmpty(TransactionId))
                {
                    EnvelopesApi envelopesApi = new EnvelopesApi();
                    EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountId, TransactionId);
                    
                }                
                

                return sendAlerts;
            }).ConfigureAwait(false);

            if (tskAlerts.Any())
            {
                alerts.AddRange(tskAlerts);
            }            

            return alerts;
        }

        private bool AreRecipientsEqual(Recipients recipients)
        {            
            bool areEqual = false;

            if (recipients != null && _signers != null)
            {
                if (recipients.Signers.Count == _signers.Count)
                {
                    areEqual = _signers.All(s => recipients.Signers.Any(r => r.Email == s.Email));
                    if (areEqual && recipients.CarbonCopies.Count == _copyViewers.Count)
                    {
                        areEqual = _copyViewers.All(s => recipients.CarbonCopies.Any(r => r.Email == s.Email));
                    }
                }
            }

            return areEqual;
        }

        private List<Alert> CreateEnvelope(EnvelopeDefinition envelopeDefinition)
        {
            List<Alert> alerts = new List<Alert>();

            EnvelopesApi envelopesApi = new EnvelopesApi();
            EnvelopeSummary envelopeSummary = envelopesApi.CreateEnvelope(AccountId, envelopeDefinition);

            if (!string.IsNullOrEmpty(envelopeSummary?.EnvelopeId))
            {
                EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountId, envelopeSummary.EnvelopeId);

                TransactionId = envelopeSummary.EnvelopeId;
                _loggingService.LogInfo($"DocuSign document was successfully created: envelope {TransactionId}, document {docsList?.EnvelopeDocuments?.FirstOrDefault()?.DocumentId}");
                DocumentId = docsList?.EnvelopeDocuments?.FirstOrDefault()?.DocumentId;
            }
            else
            {
                _loggingService.LogError("DocuSign document wasn't created");
                alerts.Add(new Alert()
                {
                    Type = AlertType.Error,
                    Header = "DocuSign error",
                    Message = "DocuSign document wasn't created"
                });
            }

            return alerts;
        }

        private EnvelopeDefinition PrepareEnvelope()
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition()
            {
                EmailSubject = EmailSubject
            };

            if (_templateUsed)
            {
                FillEnvelopeForTemplate(envelopeDefinition);
            }
            else
            {
                envelopeDefinition.CompositeTemplates = new List<CompositeTemplate>()
                {
                    new CompositeTemplate()
                    {
                        InlineTemplates = new List<InlineTemplate>()
                        {
                            new InlineTemplate()
                            {
                                Sequence = "1",
                                Recipients = new Recipients()
                                {
                                    Signers = _signers,
                                    CarbonCopies = _copyViewers
                                }
                            }
                        },
                        Document = _document
                    },
                };
            }
            envelopeDefinition.Status = "sent";

            return envelopeDefinition;
        }

        private void FillEnvelopeForTemplate(EnvelopeDefinition envelopeDefinition)
        {
            List<TemplateRole> rolesList = new List<TemplateRole>();

            _signers.ForEach(signer =>
            {
                rolesList.Add(new TemplateRole()
                {
                    Name = signer.Name,
                    Email = signer.Email,
                    RoutingOrder = signer.RoutingOrder,
                    RoleName = $"Signer{signer.RoutingOrder}",
                    Tabs = signer.Tabs
                });
            });            

            _copyViewers.ForEach(viewer =>
            {
                rolesList.Add(new TemplateRole()
                {
                    Email = viewer.Email,
                    Name = viewer.Name,
                    RoutingOrder = viewer.RoutingOrder,
                    RoleName = "Viewer",                    
                });
            });

            envelopeDefinition.TemplateId = _templateId;
            envelopeDefinition.TemplateRoles = rolesList;            
        }

        private string CreateAuthHeader(string userName, string password, string integratorKey)
        {
            DocuSignCredentials dsCreds = new DocuSignCredentials()
            {
                Username = userName,
                Password = password,
                IntegratorKey = integratorKey
            };

            string authHeader = Newtonsoft.Json.JsonConvert.SerializeObject(dsCreds);
            return authHeader;
        }

        private Signer CreateSigner(SignatureUser signatureUser, int routingOrder)
        {
            var signer = new Signer()
            {
                Email = signatureUser.EmailAddress,
                Name = $"{signatureUser.FirstName} {signatureUser.LastName}",
                RecipientId = routingOrder.ToString(),
                RoutingOrder = routingOrder.ToString(),
                Tabs = new Tabs()
                {
                    SignHereTabs = new List<SignHere>()
                    {
                        new SignHere()
                        {
                            TabLabel = $"Signature{routingOrder}"
                        }
                    }
                }
            };
            if (routingOrder == 1)
            {
                if (_textTabs?.Any() ?? false)
                {
                    if (signer.Tabs.TextTabs == null)
                    {
                        signer.Tabs.TextTabs = new List<Text>();
                    }
                    signer.Tabs.TextTabs.AddRange(_textTabs);
                }
                if (_checkboxTabs?.Any() ?? false)
                {
                    if (signer.Tabs.CheckboxTabs == null)
                    {
                        signer.Tabs.CheckboxTabs = new List<Checkbox>();
                    }
                    signer.Tabs.CheckboxTabs.AddRange(_checkboxTabs);
                }
            }

            return signer;
        }
    }

    public class DocuSignCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IntegratorKey { get; set; }
    }
}
