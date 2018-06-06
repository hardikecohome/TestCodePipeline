using System;

namespace DealnetPortal.Api.BackgroundScheduler
{
    public interface IBackgroundSchedulerService
    {
        void CheckExpiredLeads(DateTime currentDateTime, int minutesPeriod);
    }
}
