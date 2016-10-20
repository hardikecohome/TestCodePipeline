using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Models;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;

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

        public Task<IList<Alert>> InsertDocumentFields(IList<FormField> formFields)
        {
            throw new NotImplementedException();
        }

        public void InsertSignatures()
        {
            throw new NotImplementedException();
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
    }
}
