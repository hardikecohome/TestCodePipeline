using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireService : IAspireService
    {
        private readonly IAspireServiceAgent _aspireServiceAgent;
        private readonly ILoggingService _loggingService;

        public AspireService(IAspireServiceAgent aspireServiceAgent, ILoggingService loggingService)
        {
            _aspireServiceAgent = aspireServiceAgent;
            _loggingService = loggingService;
        }
    }
}
