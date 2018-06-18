using System.Data.Entity;

namespace DealnetPortal.DataAccess
{
    public class HangfireContext : DbContext
    {
        public HangfireContext() : base("DefaultConnection")
        {
            Database.SetInitializer<HangfireContext>(null);
            Database.CreateIfNotExists();
        }
    }
}