using System.Data.Entity;
using DealnetPortal.Domain;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DealnetPortal.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }
        
        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        /// <summary>
        /// temporary added virtual
        /// </summary>
        public virtual DbSet<Contract> Contracts { get; set; }

        public virtual DbSet<Customer> Customers { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<Contract>()
            //    .HasMany<Customer>(c => c.Customers).WithRequired(ho => ho.Contract).WillCascadeOnDelete(true);
            base.OnModelCreating(modelBuilder);
        }

        //public DbSet<ContractAddress> ContractAddresses { get; set; }
    }
}