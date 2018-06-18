using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DealnetPortal.Api.BackgroundScheduler;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.DataAccess;
using DealnetPortal.Utilities.Configuration;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Owin;
using Unity;

namespace DealnetPortal.Api
{
    public partial class Startup
    {
        public void ConfigurationScheduler(IAppBuilder app)
        {
            try
            {
                var config = (IAppConfiguration)System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAppConfiguration)) ?? new AppConfiguration(WebConfigSections.AdditionalSections);
                GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection", new SqlServerStorageOptions{QueuePollInterval = TimeSpan.FromMinutes(30)});
                GlobalConfiguration.Configuration.UseUnityActivator(UnityConfig.GetConfiguredContainer());

                var monitor = JobStorage.Current.GetMonitoringApi();
                monitor.DeactivateAllJobs();
                DisposeServers();

                RecurringJob.AddOrUpdate<IBackgroundSchedulerService>(service =>
                service.CheckExpiredLeads(DateTime.UtcNow, int.Parse(config.GetSetting(WebConfigKeys.LEAD_EXPIREDMINUTES_CONFIG_KEY))), Cron.MinuteInterval(int.Parse(config.GetSetting(WebConfigKeys.LEAD_CHECKPERIODMINUTES_CONFIG_KEY))));

                //app.UseHangfireDashboard(); //for test purposes
                var serverOptions = new BackgroundJobServerOptions { WorkerCount = 1 };
                app.UseHangfireServer(serverOptions);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal static bool DisposeServers()
        {
            try
            {
                var type = Type.GetType("Hangfire.AppBuilderExtensions, Hangfire.Core", throwOnError: false);
                if (type == null) return false;

                var field = type.GetField("Servers", BindingFlags.Static | BindingFlags.NonPublic);
                if (field == null) return false;

                var value = field.GetValue(null) as ConcurrentBag<BackgroundJobServer>;
                if (value == null) return false;

                var servers = value.ToArray();

                foreach (var server in servers)
                {
                    // Dispose method is a blocking one. It's better to send stop
                    // signals first, to let them stop at once, instead of one by one.
                    server.SendStop();
                }

                foreach (var server in servers)
                {
                    server.Dispose();
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}