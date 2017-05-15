using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.Integration.Services
{
    public class CustomerWalletService : ICustomerWalletService
    {
        private readonly ICustomerWalletServiceAgent _customerWalletServiceAgent;
        private readonly IContractRepository _contractRepository;
        private readonly ILoggingService _loggingService;

        public CustomerWalletService(ICustomerWalletServiceAgent customerWalletServiceAgent, IContractRepository contractRepository, ILoggingService loggingService)
        {
            _customerWalletServiceAgent = customerWalletServiceAgent;
            _contractRepository = contractRepository;
            _loggingService = loggingService;
        }

        public async Task<IList<Alert>> CreateCustomerByContract(DealnetPortal.Domain.Contract contract, string contractOwnerId)
        {
            var alerts = new List<Alert>();
            return alerts;
        }
    }
}
