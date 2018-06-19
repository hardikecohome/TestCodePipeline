using System;
using System.Web.Http.Dependencies;
using DealnetPortal.Api.BackgroundScheduler;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Utilities.Configuration;
using Hangfire;
using Hangfire.Storage;
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
                CleanAllJobs();
                RecurringJob.AddOrUpdate<IBackgroundSchedulerService>(b =>
                        b.CheckExpiredLeads(DateTime.UtcNow, int.Parse(config.GetSetting(WebConfigKeys.LEAD_EXPIREDMINUTES_CONFIG_KEY))),
                    Cron.MinuteInterval(int.Parse(config.GetSetting(WebConfigKeys.LEAD_CHECKPERIODMINUTES_CONFIG_KEY))));

                app.UseHangfireDashboard();
                app.UseHangfireServer();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void CleanAllJobs()
        {
            using (var connection = JobStorage.Current.GetConnection())
            {
                foreach (var recurringJob in connection.GetRecurringJobs())
                {
                    RecurringJob.RemoveIfExists(recurringJob.Id);
                }
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