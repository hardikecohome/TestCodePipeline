using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Models;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class SignatureService : ISignatureService
    {
        private readonly IESignatureServiceAgent _signatureServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;
        private readonly IFileRepository _fileRepository;

        private List<string> _fieldNames = new List<string>()
        {
            "First Name", "Last Name"
        }

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


            return alerts;
        }
    }
}
