using System;
using System.Web.Http.Dependencies;
using DealnetPortal.Api.BackgroundScheduler;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Utilities.Configuration;
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Owin;

namespace DealnetPortal.Api
{
    public partial class Startup
    {

        public void ConfigurationScheduler(IAppBuilder app)
        {
            try
            {
                var config = (IAppConfiguration)System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAppConfiguration))
                    ?? new AppConfiguration(WebConfigSections.AdditionalSections);
                GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
                //GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator());
                GlobalConfiguration.Configuration.UseActivator(new UnityJobActivator(System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver));

                var monitor = JobStorage.Current.GetMonitoringApi();
                monitor.DeactivateAllJobs();
                DisposeServers();

                RecurringJob.AddOrUpdate<IBackgroundSchedulerService>(service =>
                    service.CheckExpiredLeads(DateTime.UtcNow, int.Parse(config.GetSetting(WebConfigKeys.LEAD_EXPIREDMINUTES_CONFIG_KEY))),
                
                    Cron.MinuteInterval(int.Parse(config.GetSetting(WebConfigKeys.LEAD_CHECKPERIODMINUTES_CONFIG_KEY))));

                app.UseHangfireDashboard();
                app.UseHangfireServer();

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


        //private static void CleanAllJobs()
        //{
        //    using (var connection = JobStorage.Current.GetConnection())
        //    {
        //        foreach (var recurringJob in connection.GetRecurringJobs())
        //        {
        //            RecurringJob.RemoveIfExists(recurringJob.Id);
        //        }
        //    }
        //}
    }

    public static class HangfireExtensions
    {
        public static void DeactivateAllJobs(this IMonitoringApi monitor)
        {
            var toDelete = new List<string>();

            foreach (QueueWithTopEnqueuedJobsDto queue in monitor.Queues())
            {
                for (var i = 0; i < Math.Ceiling(queue.Length / 1000d); i++)
                {
                    monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
                        .ForEach(x => toDelete.Add(x.Key));

                }
            }
            monitor.ScheduledJobs(0, (int)monitor.ScheduledCount())
                      .ForEach(x => toDelete.Add(x.Key));
            foreach (var jobId in toDelete)
            {
                BackgroundJob.Delete(jobId);
            }
        }
    }

    public class ContainerJobActivator : JobActivator
    {
        public ContainerJobActivator()
        {
        }

        public override object ActivateJob(Type type)
        {
            return System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver.GetService(type);
        }
    }

    public class UnityJobActivator : JobActivator
    {
        private readonly IDependencyResolver _dependencyResolver;

        /// <summary>
        /// Initialize a new instance of the <see cref="T:UnityJobActivator"/> class
        /// </summary>
        /// <param name="container">The unity container to be used</param>

        public UnityJobActivator(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver ?? throw new ArgumentNullException(nameof(dependencyResolver));
        }


        /// <inheritdoc />
        public override object ActivateJob(Type jobType)
        {
            return _dependencyResolver.GetService(jobType);
        }

        public override JobActivatorScope BeginScope(JobActivatorContext context)
        {
            return new UnityScope(_dependencyResolver.BeginScope());
        }

        class UnityScope : JobActivatorScope
        {
            private readonly IDependencyScope _dependencyScope;

            public UnityScope(IDependencyScope dependencyScope)
            {
                _dependencyScope = dependencyScope;
            }

            public override object Resolve(Type type)
            {
                return _dependencyScope.GetService(type);
            }

            public override void DisposeScope()
            {
                _dependencyScope.Dispose();
            }
        }
    }

}