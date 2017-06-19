using DealnetPortal.Utilities.Logging;
using System;
using System.Linq;
using System.Web.Http;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Api.Integration.Services;

namespace DealnetPortal.Api.BackgroundScheduler
{
    public class BackgroundSchedulerService : IBackgroundSchedulerService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IMailService _mailService;
        private readonly ILoggingService _loggingService;

        public BackgroundSchedulerService()
        {
            _loggingService = (ILoggingService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));
            _contractRepository = (IContractRepository)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IContractRepository));
            _mailService = (IMailService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IMailService));
        }

        public void CheckExpiredLeads(DateTime currentDateTime, int minutesPeriod)
        {
            _loggingService.LogInfo($"Checking expired leads started at {DateTime.Now}.");
            var expiredDateTime = currentDateTime.AddMinutes(-minutesPeriod);
            try
            {
                var contracts = _contractRepository.GetExpiredContracts(expiredDateTime);
                foreach (var contract in contracts)
                {
                    _mailService.SendNotifyMailNoDealerAcceptedLead12H(contract);
                }
                _loggingService.LogInfo($"Checking expired leads finished at {DateTime.Now}. There are {contracts.Count} expired contracts");
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in CheckExpiredLeads", ex);
            }
            _loggingService.LogInfo($"Checking expired leads finished at {DateTime.Now}.");
        }
    }
}
