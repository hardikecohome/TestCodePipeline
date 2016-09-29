using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Models;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;

        //private List<string> _fieldNames = new List<string>()
        //{
        //    "First Name", "Last Name"
        //}

        public SignatureService(IESignatureServiceAgent signatureServiceAgent, IContractRepository contractRepository,
                                IFileRepository fileRepository, ILoggingService loggingService)
        {
            _signatureServiceAgent = signatureServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
            _fileRepository = fileRepository;
        }

        public IList<Alert> ProcessContract(int contractId, string ownerUserId)
        {
            IList<Alert> alerts = new List<Alert>();

            // Get contract
            var contract = _contractRepository.GetContractAsUntracked(contractId, ownerUserId);
            if (contract != null)
            {
                _loggingService.LogInfo($"Started eSignature processing for contract [{contractId}]");
                var fields = PrepareFormFields(contract);
                _loggingService.LogInfo($"{fields.Count} fields collected");
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

        private Dictionary<string, string> PrepareFormFields(Contract contract)
        {
            var fields = new Dictionary<string, string>();

            FillHomeOwnerFieilds(fields, contract);

            return fields;
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
                    
                }
            }
        }
    }
}
