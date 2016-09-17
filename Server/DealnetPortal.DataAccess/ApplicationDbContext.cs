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

        public virtual DbSet<Location> Locations { get; set; }

        public virtual DbSet<Phone> Phones { get; set; }

        public virtual DbSet<PaymentInfo> PaymentInfos { get; set; }

        public virtual DbSet<ContactInfo> ContactInfos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<Contract>()
            //    .HasMany<Customer>(c => c.AdditionalApplicants).WithRequired(ho => ho.Contract).WillCascadeOnDelete(true);
            base.OnModelCreating(modelBuilder);
        }

        //public DbSet<Location> ContractAddresses { get; set; }
    }
}