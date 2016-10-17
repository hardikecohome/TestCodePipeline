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

        public virtual DbSet<Email> Emails { get; set; }

        public virtual DbSet<ContractDocument> ContractDocuments { get; set; }

        public virtual DbSet<EquipmentInfo> EquipmentInfo { get; set; }
        public virtual DbSet<NewEquipment> NewEquipment { get; set; }
        public virtual DbSet<ExistingEquipment> ExistingEquipment { get; set; }

        public virtual DbSet<PaymentInfo> PaymentInfos { get; set; }

        public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }

        public DbSet<AgreementTemplate> AgreementTemplates { get; set; }

        public virtual DbSet<ProvinceTaxRate> ProvinceTaxRates { get; set; }

        public virtual DbSet<DocumentType> DocumentTypes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            //modelBuilder.Entity<Contract>()
            //    .HasMany<Customer>(c => c.AdditionalApplicants).WithRequired(ho => ho.Contract).WillCascadeOnDelete(true);
            base.OnModelCreating(modelBuilder);
        }

        //public DbSet<Location> ContractAddresses { get; set; }
    }
}