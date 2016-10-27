using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Cryptography;
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

        public override void InitializeDatabase(ApplicationDbContext context)
        {
            base.InitializeDatabase(context);
        }


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

            //var subUser1 = new ApplicationUser()
            //{
            //    Email = "Winnie Pooh",
            //    UserName = "Winnie Pooh",
            //    Application = applications.First(x => x.Id == "df460bb2-f880-42c9-aae5-9e3c76cdcd0f"),
            //    EmailConfirmed = true,
            //    PhoneNumberConfirmed = false,
            //    TwoFactorEnabled = false,
            //    LockoutEnabled = false,
            //    AccessFailedCount = 0,
            //    PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
            //    //Password: 123_Qwe
            //    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            //};
            //var subUser2 = new ApplicationUser()
            //{
            //    Email = "Mickey Mouse",
            //    UserName = "Mickey Mouse",
            //    Application = applications.First(x => x.Id == "df460bb2-f880-42c9-aae5-9e3c76cdcd0f"),
            //    EmailConfirmed = true,
            //    PhoneNumberConfirmed = false,
            //    TwoFactorEnabled = false,
            //    LockoutEnabled = false,
            //    AccessFailedCount = 0,
            //    PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
            //    //Password: 123_Qwe
            //    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            //};
            //user1.SubDealers = new HashSet<ApplicationUser>();
            //user1.SubDealers.Add(subUser1);
            //user1.SubDealers.Add(subUser2);            
        }

        private void SetAspireTestUsers(ApplicationDbContext context, Application[] applications)
        {
            //EcoSmarts users
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
                DisplayName = "Eco Smart Home Services",
                AspireAccountId = "70017",
                AspireLogin = "ecosmart",
                AspirePassword = "123456789"
            };
            context.Users.Add(ecosmartUser);
            var canadianhomeUser = new ApplicationUser()
            {
                Email = "canadianhome@eco.com",
                UserName = "canadianhome@eco.com",
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
                DisplayName = "Canadian Home Efficiency Services",
                AspireAccountId = "70122",
                AspireLogin = "canadianhome",
                AspirePassword = "123456789"
            };
            context.Users.Add(canadianhomeUser);
            var enertechUser = new ApplicationUser()
            {
                Email = "enertech@eco.com",
                UserName = "enertech@eco.com",
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
                DisplayName = "Enertech Home Services",
                AspireAccountId = "70133",
                AspireLogin = "enertech",
                AspirePassword = "123456789"
            };
            context.Users.Add(enertechUser);
            var efficiencyUser = new ApplicationUser()
            {
                Email = "efficiency@eco.com",
                UserName = "efficiency@eco.com",
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
                DisplayName = "Efficiency Standard Home Services",
                AspireAccountId = "70116",
                AspireLogin = "efficiency",
                AspirePassword = "123456789"
            };
            context.Users.Add(efficiencyUser);
            
            //EcoEnergy users
            var ecoenergyUser = new ApplicationUser()
            {
                Email = "ecoenergy@eco.com",
                UserName = "ecoenergy@eco.com",
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
                DisplayName = "Eco Energy Home Services",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };

            ecoenergyUser.SubDealers = new HashSet<ApplicationUser>();
            var ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Apex Home Services",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Apex Home Services",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ontario Safety Standards",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ontario Safety Standards",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ikotel O/A Ontario Water Health Safety",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ikotel O/A Ontario Water Health Safety",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ontario Green Solutions",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ontario Green Solutions",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "EcoLife",
                Application = applications.First(x => x.Id == EcohomeAppId),
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "EcoLife",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);

            context.Users.Add(ecoenergyUser);
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

        private static string HashPassword(string password)
        {
            byte[] salt;
            byte[] buffer2;
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }
            using (Rfc2898DeriveBytes bytes = new Rfc2898DeriveBytes(password, 0x10, 0x3e8))
            {
                salt = bytes.Salt;
                buffer2 = bytes.GetBytes(0x20);
            }
            byte[] dst = new byte[0x31];
            Buffer.BlockCopy(salt, 0, dst, 1, 0x10);
            Buffer.BlockCopy(buffer2, 0, dst, 0x11, 0x20);
            return Convert.ToBase64String(dst);
        }

    }
}
