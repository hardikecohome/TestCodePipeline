using System.Data.Entity;
using System.Web.Http;
using Crypteron;
using DealnetPortal.DataAccess;
using DealnetPortal.Utilities.Logging;

namespace DealnetPortal.Api.App_Start
{
    public class DatabaseConfig
    {
        public static void Initialize()
        {
            CrypteronConfig.Config.CommonConfig.AllowNullsInSecureFields = true;            

            var loggingService =
                (ILoggingService)
                    GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(ILoggingService));
            Database.SetInitializer(new MigrateDatabaseToLatestVersionWithLog<ApplicationDbContext, DataAccess.Migrations.Configuration>(loggingService));
            //Force migration
            //var dbMigrator = new DbMigrator(new DealnetPortal.DataAccess.Migrations.Configuration());
            //dbMigrator.Update();
        }
    }
}