using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using DealnetPortal.Utilities;

namespace DealnetPortal.DataAccess
{
    public class MigrateDatabaseToLatestVersionWithLog<TContext, TMigrationsConfiguration>
        : MigrateDatabaseToLatestVersion<TContext, TMigrationsConfiguration> 
        where TContext : System.Data.Entity.DbContext
        where TMigrationsConfiguration : System.Data.Entity.Migrations.DbMigrationsConfiguration<TContext>, new()
    {
        private ILoggingService _loggingService;

        public MigrateDatabaseToLatestVersionWithLog(ILoggingService loggingService)
        {
            _loggingService = loggingService;
            //_loggingService.LogInfo(Directory.Get());
            //try
            //{
            //    var dir = HostingEnvironment.MapPath("~/SeedData");
            //   //var dir = AppDomain.CurrentDomain.BaseDirectory;
            //    var path = dir + "//test.pdf";//Path.Combine("SeedData", "test" + ".pdf");
            //    File.ReadAllBytes(path);
            //}
            //catch (Exception ex)
            //{
            //    _loggingService.LogError("",ex);
            //    // ignored
            //}
        }
        public override void InitializeDatabase(TContext context)
        {
            try
            {
                base.InitializeDatabase(context);
            }
            catch (Exception ex)
            {
                _loggingService?.LogError("Failed to Initialize database", ex);
            }
        }
    }
}
