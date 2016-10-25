using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Signature;
using DealnetPortal.Domain;
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

        public string AccountId { get; private set; }        

        public string TransactionId { get; private set; }

        public string DocumentId { get; private set; }

        private Document _document { get; set; }

        private List<Text> _textTabs { get; set; }

        private List<Checkbox> _checkboxTabs { get; set; } 


        public DocuSignSignatureEngine()
        {
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
                    logAlerts.Add(new Alert()
                    {
                        Type = AlertType.Error,
                        Header = "DocuSign error",
                        Message = "Can't login to DocuSign service"
                    });
                }

                return logAlerts;
            });

            if (loginAlerts.Any())
            {
                alerts.AddRange(loginAlerts);
            }

            return alerts;
        }

        public async Task<IList<Alert>> StartNewTransaction(Contract contract, AgreementTemplate agreementTemplate)
        {
            var alerts = new List<Alert>();

            await Task.Run(() =>
            {

                if (contract != null & agreementTemplate != null)
                {
                    _document = new Document();
                    _document.DocumentBase64 = System.Convert.ToBase64String(agreementTemplate.AgreementForm);
                    _document.Name = agreementTemplate.TemplateName;
                    _document.DocumentId = contract.Id.ToString();
                    _document.TransformPdfFields = "true";
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
            });

            return alerts;
        }

        public Task<IList<Alert>> InsertSignatures(IList<SignatureUser> signatureUsers)
        {
            throw new NotImplementedException();
        }

        public Task<IList<Alert>> SendInvitations(IList<SignatureUser> signatureUsers)
        {
            throw new NotImplementedException();
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
    }

    public class DocuSignCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string IntegratorKey { get; set; }
    }
}
