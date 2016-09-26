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

        public SignatureService(IESignatureServiceAgent signatureServiceAgent, IContractRepository contractRepository,
            ILoggingService loggingService)
        {
            _signatureServiceAgent = signatureServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
        }

        public IList<Alert> ProcessContract(int contractId, string ownerUserId)
        {
            IList<Alert> alerts = new List<Alert>();


            return alerts;
        }
    }
}
