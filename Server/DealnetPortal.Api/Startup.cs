using System;
using System.Diagnostics;
using Hangfire;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(DealnetPortal.Api.Startup))]

namespace DealnetPortal.Api
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");

            BackgroundJob.Enqueue(() => Debug.WriteLine("Getting Started with HangFire!"));
            RecurringJob.AddOrUpdate(() => Debug.WriteLine("This job will execute once in every minute"), Cron.Minutely);

            app.UseHangfireDashboard();
            app.UseHangfireServer();
        }
    }
}
