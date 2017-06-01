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
        private ILoggingService _loggingService;

        public BackgroundSchedulerService()
        {
            _loggingService = (ILoggingService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));
            _contractRepository = (IContractRepository)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IContractRepository));
            _mailService = (IMailService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IMailService));
        }

        public void CheckExpiredLeads(DateTime currentDateTime, int hoursPeriod)
        {
            var expiredDateTime = currentDateTime.AddHours(-hoursPeriod);
            try
            {
                var contracts = _contractRepository.GetExpiredContracts(expiredDateTime);
                //foreach (var contract in contracts)
                {
                    _mailService.SendNotifyMailNoDealerAcceptedLead12H(contracts.FirstOrDefault());
                }
            }
            catch (Exception ex)
            {
                _loggingService.LogError("Error in CheckExpiredLeads", ex);
            }
        }
    }
}
