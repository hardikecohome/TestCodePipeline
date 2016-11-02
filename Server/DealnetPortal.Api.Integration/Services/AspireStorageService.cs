using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.Integration.Services
{
    public class AspireStorageService : IAspireStorageService
    {
        private readonly ILoggingService _loggingService;
        public AspireStorageService(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

    }
}
