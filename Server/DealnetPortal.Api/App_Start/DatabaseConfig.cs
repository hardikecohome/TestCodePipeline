using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using DealnetPortal.DataAccess;
using DealnetPortal.Utilities;

namespace DealnetPortal.Api.App_Start
{
    public class DatabaseConfig
    {
        public static void Initialize()
        {
            //Database.SetInitializer(
            //    new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());
            var loggingService =
                (ILoggingService)
                    GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));

            //Database.SetInitializer(
            //    new DropCreateDbWithSeedTestData(loggingService));
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, DealnetPortal.DataAccess.Migrations.Configuration>());

        }
    }
}