using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Api.Models.Storage;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.Logging;
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

        private readonly string _baseServerAddress;

        private readonly ILoggingService _loggingService;

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

        //private const string EmailSubject = "Please Sign Agreement";

        private const string NotificationEndpointName = "api/Storage/NotifySignatureStatus";

        public DocuSignSignatureEngine(ILoggingService loggingService, IAppConfiguration configuration)
        {
            _loggingService = loggingService;
            _baseUrl = configuration.GetSetting(WebConfigKeys.DOCUSIGN_APIURL_CONFIG_KEY);
            _dsUser = configuration.GetSetting(WebConfigKeys.DOCUSIGN_USER_CONFIG_KEY);
            _dsPassword = configuration.GetSetting(WebConfigKeys.DOCUSIGN_PASSWORD_CONFIG_KEY);
            _dsIntegratorKey = configuration.GetSetting(WebConfigKeys.DOCUSIGN_INTEGRATORKEY_CONFIG_KEY);

            _baseServerAddress = configuration.GetSetting(WebConfigKeys.SERVER_BASE_ADDRESS_CONFIG_KEY);
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
                if (contract != null & agreementTemplate?.TemplateDocument != null)
                {
                    if (!string.IsNullOrEmpty(agreementTemplate.TemplateDocument.ExternalTemplateId))
                    {
                        _templateId = agreementTemplate.TemplateDocument.ExternalTemplateId;
                        _templateUsed = true;
                    }
                    else
                    {
                        _templateUsed = false;
                        _document = new Document
                        {
                            DocumentBase64 = System.Convert.ToBase64String(agreementTemplate.TemplateDocument.TemplateBinary),
                            Name = agreementTemplate.TemplateDocument.TemplateName,
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
                    signatureUsers.Where(s => s.Role == SignatureRole.Dealer).ForEach(s =>
                    {
                        _signers.Add(CreateSigner(s, signN++, true));
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
                            RoleName = "Viewer"
                        });
                        signN++;
                    });
                }

            });

            return alerts;
        }        

        public async Task<IList<Alert>> SubmitDocument(IList<SignatureUser> signatureUsers)
        {
            var alerts = new List<Alert>();

            await Task.Run(() =>
            {
                EnvelopesApi envelopesApi = new EnvelopesApi();
                bool recreateEnvelope = false;
                bool recreateRecipients = false;

                if (!string.IsNullOrEmpty(TransactionId))
                {
                    var envelope = envelopesApi.GetEnvelope(AccountId, TransactionId);
                    var reciepents = envelopesApi.ListRecipients(AccountId, TransactionId);
                    if (envelope != null)
                    {
                        //TODO: can't sign a draft - have to deal with it
                        if (envelope.Status == "created")
                        {
                            recreateEnvelope = true;
                        }
                        //await InsertSignatures(signatureUsers);// ??
                        recreateRecipients = !AreRecipientsEqual(reciepents);
                        if ((envelope.Status == "sent" || envelope.Status == "completed") && recreateRecipients)
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
                        try
                        {                        
                            if (recreateRecipients)
                            {
                                if (reciepents.Signers.Any() || reciepents.CarbonCopies.Any())
                                {
                                    var delRec = envelopesApi.DeleteRecipients(AccountId, TransactionId, reciepents);
                                }
                                reciepents = new Recipients()
                                {
                                    Signers = _signers,
                                    CarbonCopies = _copyViewers
                                };
                                var updateRes = envelopesApi.UpdateRecipients(AccountId, TransactionId, reciepents);
                            }
                            if (envelope.Status == "created")
                            {
                                envelope = new Envelope();
                                envelope.Status = "sent";                                
                                var updateEnvelopeRes =                                    
                                        envelopesApi.Update(AccountId, TransactionId, envelope);
                            }
                            else
                            {
                                alerts.Add(new Alert()
                                {
                                    Type = AlertType.Warning,
                                    Header = "eSignature Warning",
                                    Message = "Agreement has been sent for signature already"
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            alerts.Add(new Alert()
                            {
                                Type = AlertType.Error,
                                Header = "eSignature error",
                                Message = ex.ToString()
                            });
                        }
                    }
                }
                else
                {
                    recreateEnvelope = true;
                }                

                if (recreateEnvelope)
                {                    
                    if (!_signers.Any() && !_copyViewers.Any())
                    {                        
                        var tempSignatures = new List<SignatureUser>
                        {
                            new SignatureUser()
                            {
                                Role = SignatureRole.Signer
                            }
                        };
                        InsertSignatures(tempSignatures).GetAwaiter().GetResult();
                        _envelopeDefinition = PrepareEnvelope("created");
                    }
                    else
                    {
                        _envelopeDefinition = PrepareEnvelope();
                    }                    

                    var envAlerts = CreateEnvelope(_envelopeDefinition);
                    if (envAlerts.Any())
                    {
                        alerts.AddRange(envAlerts);
                    }
                }

            });
                                  
            return alerts;
        }        

        public async Task<Tuple<AgreementDocument, IList<Alert>>> GetDocument(DocumentVersion documentVersion)
        {
            var alerts = new List<Alert>();
            AgreementDocument document = null;

            var tskAlerts = await Task.Run(async () =>
            {
                var sendAlerts = new List<Alert>();

                if (!string.IsNullOrEmpty(TransactionId))
                {
                    EnvelopesApi envelopesApi = new EnvelopesApi();
                    EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountId, TransactionId);
                    if (docsList.EnvelopeDocuments.Any())
                    {
                        var doc = docsList.EnvelopeDocuments.First();
                        document = new AgreementDocument()
                        {
                            DocumentId = doc.DocumentId,
                            Type = doc.Type,
                            Name = doc.Name
                        };                        
                        var docStream = envelopesApi.GetDocument(AccountId, TransactionId, doc.DocumentId);
                        document.DocumentRaw = new byte[docStream.Length];
                        await docStream.ReadAsync(document.DocumentRaw, 0, (int)docStream.Length);
                    }                    
                }                               
                return sendAlerts;
            });

            if (tskAlerts.Any())
            {
                alerts.AddRange(tskAlerts);
            }            

            return new Tuple<AgreementDocument, IList<Alert>>(document, alerts);
        }

        private bool AreRecipientsEqual(Recipients recipients)
        {            
            bool areEqual = false;

            if (recipients != null && _signers != null)
            {
                areEqual = _signers.All(s => recipients.Signers.Any(r => r.Email == s.Email))
                                && (_copyViewers?.All(s => recipients.CarbonCopies?.Any(r => r.Email == s.Email) ?? true) ?? true);
                
                //if (recipients.Signers.Count == _signers.Count)
                //{
                //    areEqual = _signers.All(s => recipients.Signers.Any(r => r.Email == s.Email));
                //    if (areEqual && recipients.CarbonCopies.Count == _copyViewers.Count)
                //    {
                //        areEqual = _copyViewers.All(s => recipients.CarbonCopies.Any(r => r.Email == s.Email));
                //    }
                //}
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

        private EnvelopeDefinition PrepareEnvelope(string statusOnCreation = "sent")
        {
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition()
            {
                EmailSubject = Resources.Resources.PleaseSignAgreement
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
            envelopeDefinition.Status = statusOnCreation;
            envelopeDefinition.EventNotification = GetEventNotification();

            return envelopeDefinition;
        }

        private EventNotification GetEventNotification()
        {
            
            if (!string.IsNullOrEmpty(_baseServerAddress))
            {
                string url = string.Empty;
                url = string.Format("{0}/{1}", _baseServerAddress, NotificationEndpointName);
                _loggingService.LogInfo($"DocuSign notofocations will send to {url}");

                List<EnvelopeEvent> envelope_events = new List<EnvelopeEvent>();

                EnvelopeEvent envelope_event1 = new EnvelopeEvent();
                envelope_event1.EnvelopeEventStatusCode = "sent";
                envelope_events.Add(envelope_event1);
                EnvelopeEvent envelope_event2 = new EnvelopeEvent();
                envelope_event2.EnvelopeEventStatusCode = "delivered";
                envelope_events.Add(envelope_event2);
                EnvelopeEvent envelope_event3 = new EnvelopeEvent();
                envelope_event3.EnvelopeEventStatusCode = "completed";
                envelope_events.Add(envelope_event3);
                EnvelopeEvent envelope_event4 = new EnvelopeEvent();
                envelope_event4.EnvelopeEventStatusCode = "declined";
                envelope_events.Add(envelope_event4);
                EnvelopeEvent envelope_event5 = new EnvelopeEvent();
                envelope_event5.EnvelopeEventStatusCode = "voided";
                envelope_events.Add(envelope_event5);

                List<RecipientEvent> recipient_events = new List<RecipientEvent>();
                RecipientEvent recipient_event1 = new RecipientEvent();
                recipient_event1.RecipientEventStatusCode = "Sent";
                recipient_events.Add(recipient_event1);
                RecipientEvent recipient_event2 = new RecipientEvent();
                recipient_event2.RecipientEventStatusCode = "Delivered";
                recipient_events.Add(recipient_event2);
                RecipientEvent recipient_event3 = new RecipientEvent();
                recipient_event3.RecipientEventStatusCode = "Completed";
                recipient_events.Add(recipient_event3);
                RecipientEvent recipient_event4 = new RecipientEvent();
                recipient_event4.RecipientEventStatusCode = "Declined";
                recipient_events.Add(recipient_event4);
                RecipientEvent recipient_event5 = new RecipientEvent();
                recipient_event5.RecipientEventStatusCode = "AuthenticationFailed";
                recipient_events.Add(recipient_event5);
                RecipientEvent recipient_event6 = new RecipientEvent();
                recipient_event6.RecipientEventStatusCode = "AutoResponded";
                recipient_events.Add(recipient_event6);

                EventNotification event_notification = new EventNotification();
                event_notification.Url = url;
                event_notification.LoggingEnabled = "true";
                event_notification.RequireAcknowledgment = "true";
                event_notification.UseSoapInterface = "false";
                event_notification.IncludeCertificateWithSoap = "false";
                event_notification.SignMessageWithX509Cert = "false";
                event_notification.IncludeDocuments = "true";
                event_notification.IncludeEnvelopeVoidReason = "true";
                event_notification.IncludeTimeZone = "true";
                event_notification.IncludeSenderAccountAsCustomField = "true";
                event_notification.IncludeDocumentFields = "true";
                event_notification.IncludeCertificateOfCompletion = "true";
                event_notification.EnvelopeEvents = envelope_events;
                event_notification.RecipientEvents = recipient_events;

                return event_notification;
            }
            else
            {
                return null;
            }
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
                    //RoutingOrder = signer.RoutingOrder, //?
                    RoleName = signer.RoleName ?? $"Signer{signer.RoutingOrder}",
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
                    RoleName = viewer.RoleName//"Viewer",                    
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

        private Signer CreateSigner(SignatureUser signatureUser, int routingOrder, bool isDealer = false)
        {
            var signer = new Signer()
            {
                Email = signatureUser.EmailAddress,
                Name = $"{signatureUser.FirstName} {signatureUser.LastName}",
                RecipientId = routingOrder.ToString(),
                RoutingOrder = routingOrder.ToString(), //not sure, probably 1
                RoleName = !isDealer ? $"Signer{routingOrder}" : "SignerD",
                Tabs = new Tabs()
                {
                    SignHereTabs = new List<SignHere>()
                    {
                        new SignHere()
                        {
                            TabLabel = !isDealer ? $"Signature{routingOrder}" : "SignatureD"
                        },
                        //?? for 2nd signature
                        new SignHere()
                        {
                            TabLabel = !isDealer ? $"Signature{routingOrder}_2" : "SignatureD_2"
                        }
                    }
                }
            };
            //if (routingOrder == 1)
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
