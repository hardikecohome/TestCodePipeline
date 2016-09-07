using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using DealnetPortal.DataAccess;

namespace DealnetPortal.Api.App_Start
{
    public class DatabaseConfig
    {
        public static void Initialize()
        {
            //Database.SetInitializer(
            //    new DropCreateDatabaseIfModelChanges<ApplicationDbContext>());
            Database.SetInitializer(
                new DropCreateDbWithSeedTestData());
        }
    }
}