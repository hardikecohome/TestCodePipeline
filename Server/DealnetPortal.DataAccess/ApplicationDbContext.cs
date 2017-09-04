using System;
using System.Data.Entity;
using System.Diagnostics;
using DealnetPortal.Domain;
using DealnetPortal.Domain.Dealer;
using Microsoft.AspNet.Identity.EntityFramework;

namespace DealnetPortal.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            SetupSecureDb();
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        #region private
        private void SetupSecureDb()
        {
            try
            {

                Crypteron.CipherDb.Session.Create(this);
            }
            catch (Exception ex)
            {
                //Logging exception of secure context creation here
                Crypteron.ErrorHandling.Logging.Logger.Log($"Cannot create secure DB context: {ex}", TraceEventType.Error);
            }
        }
        #endregion

        public virtual DbSet<Application> Applications { get; set; }

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

        public virtual DbSet<AspireStatus> AspireStatuses { get; set; }

        public virtual DbSet<DocumentType> DocumentTypes { get; set; }

        public virtual DbSet<Comment> Comments { get; set; }

        public virtual DbSet<SettingItem> SettingItems { get; set; }
        public virtual DbSet<SettingValue> SettingValues { get; set; }
        public virtual DbSet<UserSettings> UserSettings { get; set; }        

        public virtual DbSet<Language> Languages { get; set; }
        public virtual DbSet<CustomerLink> CustomerLinks { get; set; }
        public virtual DbSet<DealerService> DealerServices { get; set; }
        public virtual DbSet<DealerLanguage> DealerLanguages { get; set; }

        public virtual DbSet<DealerProfile> DealerProfiles { get; set; }
        public virtual DbSet<DealerArea> DealerArears { get; set; }
        public virtual DbSet<DealerEquipment> DealerEquipments { get; set; }

        public virtual DbSet<RateCard> RateCards { get; set; }
        public virtual DbSet<Tier> Tiers { get; set; }

        public virtual DbSet<DealerInfo> DealerInfos { get; set; }
        public virtual DbSet<CompanyInfo> CompanyInfos { get; set; }
        public virtual DbSet<CompanyProvince> CompanyProvinces { get; set; }
        public virtual DbSet<OwnerInfo> OwnerInfos { get; set; }
        public virtual DbSet<ManufacturerBrand> ManufacturerBrands { get; set; }
        public virtual DbSet<ProductInfo> ProductInfos { get; set; }
        public virtual DbSet<ProductService> ProductServices { get; set; }
        public virtual DbSet<RequiredDocument> RequiredDocuments { get; set; }
        public virtual DbSet<AdditionalDocument> AdditionalDocuments { get; set; }
        public virtual DbSet<LicenseType> LicenseTypes { get; set; }
        public virtual DbSet<LicenseDocument> LicenseDocuments { get; set; }
    }
}