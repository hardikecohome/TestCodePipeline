using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using DealnetPortal.Api.BackgroundScheduler;
using Hangfire;
using Owin;

namespace DealnetPortal.Api
{
    public partial class Startup
    {
        private IBackgroundSchedulerService _backgroundSchedulerService;

        public void ConfigurationScheduler(IAppBuilder app)
        {
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            _backgroundSchedulerService = new BackgroundSchedulerService();
            var t = int.Parse(ConfigurationManager.AppSettings["LeadExpiredMinutes"]);
            RecurringJob.AddOrUpdate(() =>
            _backgroundSchedulerService.CheckExpiredLeads(DateTime.Now, t),
                Cron.MinuteInterval(int.Parse(ConfigurationManager.AppSettings["CheckPeriodMinutes"])));

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}