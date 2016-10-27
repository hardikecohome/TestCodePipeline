using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess
{
    public class DropCreateDbWithSeedTestData :
        DropCreateDatabaseIfModelChanges<ApplicationDbContext>
    {
        private const string EcohomeAppId = "df460bb2-f880-42c9-aae5-9e3c76cdcd0f";
        private const string OdiAppId = "606cfa8b-0e2c-47ef-b646-66c5f639aebd";

        protected override void Seed(ApplicationDbContext context)
        {
            var applications = SetApplications(context);
            SetTestUsers(context, applications);
            SetAspireTestUsers(context, applications);
            SetTestEquipmentTypes(context);
            SetTestProvinceTaxRates(context);
            SetDocumentTypes(context);
        }

        private Application[] SetApplications(ApplicationDbContext context)
        {
            var applications = new []
            {
               new Application { Id = EcohomeAppId, Name = "Ecohome" },
               new Application { Id = OdiAppId, Name = "ODI" }
            };
            context.Applications.AddRange(applications);
            return applications;
        }

        private void SetTestUsers(ApplicationDbContext context, Application[] applications)
        {
            var user1 = new ApplicationUser()
            {
                Email = "user@user.com",
                UserName = "user@user.com",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            var user2 = new ApplicationUser()
            {
                Email = "user2@user.com",
                UserName = "user2@user.com",
                Application = applications.First(x => x.Id == OdiAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            context.Users.Add(user1);
            context.Users.Add(user2);

            var subUser1 = new ApplicationUser()
            {
                Email = "Winnie Pooh",
                UserName = "Winnie Pooh",
                Application = applications.First(x => x.Id == "df460bb2-f880-42c9-aae5-9e3c76cdcd0f"),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            var subUser2 = new ApplicationUser()
            {
                Email = "Mickey Mouse",
                UserName = "Mickey Mouse",
                Application = applications.First(x => x.Id == "df460bb2-f880-42c9-aae5-9e3c76cdcd0f"),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            user1.SubDealers = new HashSet<ApplicationUser>();
            user1.SubDealers.Add(subUser1);
            user1.SubDealers.Add(subUser2);            
        }

        private void SetAspireTestUsers(ApplicationDbContext context, Application[] applications)
        {
            var ecosmartUser = new ApplicationUser()
            {
                Email = "ecosmart@eco.com",
                UserName = "ecosmart@eco.com",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                SubDealerName = "Eco Smart Home Services",
                AspireAccountId = "70017",
                AspireLogin = "ecosmart",
                AspirePassword = "123456789"
            };
            context.Users.Add(ecosmartUser);
        }

        private void SetTestEquipmentTypes(ApplicationDbContext context)
        {
            var equipmentTypes = new[]
            {
                new EquipmentType {Description = "Air Conditioner", Type = "ECO1"},
                new EquipmentType {Description = "Boiler", Type = "ECO2"},
                new EquipmentType {Description = "Doors", Type = "ECO3"},
                new EquipmentType {Description = "Fireplace", Type = "ECO4"},
                new EquipmentType {Description = "Furnace", Type = "ECO5"},
                new EquipmentType {Description = "HWT", Type = "ECO6"},
                new EquipmentType {Description = "Plumbing", Type = "ECO7"},
                new EquipmentType {Description = "Roofing", Type = "ECO9"},
                new EquipmentType {Description = "Siding", Type = "ECO10"},
                new EquipmentType {Description = "Tankless Water Heater", Type = "ECO11"},
                new EquipmentType {Description = "Windows", Type = "ECO13"},
                new EquipmentType {Description = "Sunrooms", Type = "ECO38"},
                new EquipmentType {Description = "Air Handler", Type = "ECO40"},
                new EquipmentType {Description = "Flooring", Type = "ECO42"},
                new EquipmentType {Description = "Porch Enclosure", Type = "ECO43"},
                new EquipmentType {Description = "Water Treatment System", Type = "ECO44"},
                new EquipmentType {Description = "Heat Pump", Type = "ECO45"},
                new EquipmentType {Description = "HRV", Type = "ECO46"},
                new EquipmentType {Description = "Bathroom", Type = "ECO47"},
                new EquipmentType {Description = "Kitchen", Type = "ECO48"},
                new EquipmentType {Description = "Hepa System", Type = "ECO49"},
                new EquipmentType {Description = "Unknown", Type = "ECO50"},
                new EquipmentType {Description = "Security System", Type = "ECO52"},
                new EquipmentType {Description = "Basement Repair", Type = "ECO55"}
            };
            context.EquipmentTypes.AddRange(equipmentTypes);
        }

        private void SetTestProvinceTaxRates(ApplicationDbContext context)
        {
            //Obtained from http://www.retailcouncil.org/quickfacts/taxrates
            var taxRates = new[]
            {
                new ProvinceTaxRate {Province = "AB", Rate = 5},
                new ProvinceTaxRate {Province = "BC", Rate = 12},
                new ProvinceTaxRate {Province = "MB", Rate = 13},
                new ProvinceTaxRate {Province = "NB", Rate = 15},
                new ProvinceTaxRate {Province = "NL", Rate = 15},
                new ProvinceTaxRate {Province = "NT", Rate = 5},
                new ProvinceTaxRate {Province = "NS", Rate = 15},
                new ProvinceTaxRate {Province = "NU", Rate = 5},
                new ProvinceTaxRate {Province = "ON", Rate = 13},
                new ProvinceTaxRate {Province = "PE", Rate = 14},
                new ProvinceTaxRate {Province = "QC", Rate = 14.975},
                new ProvinceTaxRate {Province = "SK", Rate = 10},
                new ProvinceTaxRate {Province = "YT", Rate = 5}
            };
            context.ProvinceTaxRates.AddRange(taxRates);
        }

        private void SetDocumentTypes(ApplicationDbContext context)
        {
            var documentTypes = new[]
            {
                new DocumentType()  {Description = "Signed contract", Prefix = "SC_"},
                new DocumentType()  {Description = "Signed Installation certificate", Prefix = "SIC_"},
                new DocumentType()  {Description = "Invoice", Prefix = "INV_"},
                new DocumentType()  {Description = "Copy of Void Personal Cheque", Prefix = "VPC_"},
                new DocumentType()  {Description = "Extended Warranty Form", Prefix = "EWF_"},
                new DocumentType()  {Description = "Third party verification call", Prefix = "TPV_"},
                new DocumentType()  {Description = "Other", Prefix = ""},
            };
            context.DocumentTypes.AddRange(documentTypes);
        }
    }
}
