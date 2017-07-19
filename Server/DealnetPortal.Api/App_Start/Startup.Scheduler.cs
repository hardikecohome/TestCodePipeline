using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using DealnetPortal.Api.BackgroundScheduler;
using DealnetPortal.Api.Common.Constants;
using Hangfire;
using Owin;

namespace DealnetPortal.Api
{
    public partial class Startup
    {
        private IBackgroundSchedulerService _backgroundSchedulerService;

        public void ConfigurationScheduler(IAppBuilder app)
        {
            try
            {
                GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
                _backgroundSchedulerService = new BackgroundSchedulerService();
                RecurringJob.AddOrUpdate(() =>
                _backgroundSchedulerService.CheckExpiredLeads(DateTime.Now, int.Parse(ConfigurationManager.AppSettings[WebConfigKeys.LEAD_EXPIREDMINUTES_CONFIG_KEY])),
                    Cron.MinuteInterval(int.Parse(ConfigurationManager.AppSettings[WebConfigKeys.LEAD_CHECKPERIODMINUTES_CONFIG_KEY])));

                app.UseHangfireDashboard();
                app.UseHangfireServer();
            }
            catch (Exception ex)
            {
                
            }
            
        }
    }
}