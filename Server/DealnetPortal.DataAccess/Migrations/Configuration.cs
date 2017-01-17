using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Hosting;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using Microsoft.Practices.ObjectBuilder2;

namespace DealnetPortal.DataAccess.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<DealnetPortal.DataAccess.ApplicationDbContext>
    {
        private const string EcohomeAppId = "df460bb2-f880-42c9-aae5-9e3c76cdcd0f";
        private const string OdiAppId = "606cfa8b-0e2c-47ef-b646-66c5f639aebd";

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;           
            ContextKey = "DealnetPortal.DataAccess.ApplicationDbContext";
        }

        protected override void Seed(DealnetPortal.DataAccess.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            var applications = SetApplications(context);
            SetTestUsers(context, context.Applications.Local.ToArray());
            SetAspireTestUsers(context, context.Applications.Local.ToArray());
            SetTestEquipmentTypes(context);
            SetTestProvinceTaxRates(context);
            SetDocumentTypes(context);
            var templates = SetDocuSignTemplates(context);
            SetPdfTemplates(context, templates);
            var seedNames = templates.Select(at => at.TemplateName).ToArray();
            var dbTemplateNames =
                context.AgreementTemplates.Select(at => at.TemplateName).Where(tn => seedNames.All(t => t != tn)).ToArray();           
            SetExistingPdfTemplates(context);
        }

        private Application[] SetApplications(ApplicationDbContext context)
        {
            var applications = new[]
            {
                new Application {Id = EcohomeAppId, Name = "Ecohome", LegalName = "EcoHome Financial Inc.", FinanceProgram = "EcoHome Finance Program"},
                new Application {Id = OdiAppId, Name = "ODI"}
            };

            context.Applications.AddOrUpdate(a => a.Id, applications);
            return applications;
        }

        private void SetTestUsers(ApplicationDbContext context, Application[] applications)
        {
            var user1 = new ApplicationUser()
            {
                Email = "user@user.com",
                UserName = "user@user.com",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = false,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            var user2 = new ApplicationUser()
            {
                Email = "user2@user.com",
                UserName = "user2@user.com",
                Application = applications.First(x => x.Id == OdiAppId),
                ApplicationId = applications.First(x => x.Id == OdiAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = false,
                PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                //Password: 123_Qwe
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
            };
            var users = new List<ApplicationUser>() {user1, user2};
            //leave existing users data
            users.RemoveAll(u => context.Users.Any(dbu => dbu.UserName == u.UserName));
            context.Users.AddOrUpdate(u => u.UserName, users.ToArray());
        }

        private void SetAspireTestUsers(ApplicationDbContext context, Application[] applications)
        {
            List<ApplicationUser> users = new List<ApplicationUser>();
            //EcoSmarts users
            var ecosmartUser = new ApplicationUser()
            {
                Email = "ecosmart@eco.com",
                UserName = "ecosmart",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("123456"),// "ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Eco Smart Home Services",
                AspireAccountId = "70017",
                AspireLogin = "ecosmart",
                AspirePassword = "123456"
            };
            //context.Users.Add(ecosmartUser);
            users.Add(ecosmartUser);
            var canadianhomeUser = new ApplicationUser()
            {
                Email = "canadianhome@eco.com",
                UserName = "canadianhome",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("123456789"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Canadian Home Efficiency Services",
                AspireAccountId = "70122",
                AspireLogin = "canadianhome",
                AspirePassword = "123456789"
            };
            //context.Users.Add(canadianhomeUser);
            users.Add(canadianhomeUser);
            var enertechUser = new ApplicationUser()
            {
                Email = "enertech@eco.com",
                UserName = "enertech",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("123456789"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Enertech Home Services",
                AspireAccountId = "70133",
                AspireLogin = "enertech",
                AspirePassword = "123456789"
            };
            //context.Users.Add(enertechUser);
            users.Add(enertechUser);
            var efficiencyUser = new ApplicationUser()
            {
                Email = "efficiency@eco.com",
                UserName = "efficiency",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("123456789"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Efficiency Standard Home Services",
                AspireAccountId = "70116",
                AspireLogin = "efficiency",
                AspirePassword = "123456789"
            };
            //context.Users.Add(efficiencyUser);
            users.Add(efficiencyUser);

            //EcoEnergy users
            var ecoenergyUser = new ApplicationUser()
            {
                Email = "ecoenergy@eco.com",
                UserName = "ecoenergy",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("123456789"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
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
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                EsignatureEnabled = true,
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Apex Home Services",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            users.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ontario Safety Standards",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                EsignatureEnabled = true,
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ontario Safety Standards",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            users.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ikotel O/A Ontario Water Health Safety",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                EsignatureEnabled = true,
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ikotel O/A Ontario Water Health Safety",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            users.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "Ontario Green Solutions",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                EsignatureEnabled = true,
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Ontario Green Solutions",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            users.Add(ecoenergySubUser);
            ecoenergySubUser = new ApplicationUser()
            {
                Email = "",
                UserName = "EcoLife",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                PasswordHash = "",
                EsignatureEnabled = true,
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "EcoLife",
                AspireAccountId = "70015",
                AspireLogin = "ecoenergy",
                AspirePassword = "123456789"
            };
            ecoenergyUser.SubDealers.Add(ecoenergySubUser);
            users.Add(ecoenergySubUser);
            //context.Users.Add(ecoenergyUser);
            users.Add(ecoenergyUser);

            var smartHomeUser = new ApplicationUser()
            {
                Email = "smarthome@eco.com",
                UserName = "smarthome",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("password"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Smart Home",
                AspireAccountId = "70101",
                AspireLogin = "smarthome",
                AspirePassword = "password"
            };
            users.Add(smartHomeUser);

            var ecoHomeUser = new ApplicationUser()
            {
                Email = "ecohome@eco.com",
                UserName = "Dangelo",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("dangelo"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "ECO",
                DisplayName = "Eco Home",
                AspireAccountId = "70073",
                AspireLogin = "Dangelo",
                AspirePassword = "dangelo"
            };
            users.Add(ecoHomeUser);

            var newUser = new ApplicationUser()
            {
                Email = "fahrhall@eco.com",
                UserName = "fahrhall",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("fahrhall"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "Farhall Mechanical",
                DisplayName = "Farhall Mechanical",
                AspireAccountId = "70266",
                AspireLogin = "fahrhall",
                AspirePassword = "fahrhall"
            };
            users.Add(newUser);

            newUser = new ApplicationUser()
            {
                Email = "lifetimewater@eco.com",
                UserName = "lifetimewater",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("lifetimewater"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "Life-Time Water",
                DisplayName = "Life-Time Water",
                AspireAccountId = "70182",
                AspireLogin = "lifetimewater",
                AspirePassword = "lifetimewater"
            };
            users.Add(newUser);

            newUser = new ApplicationUser()
            {
                Email = "phphome@eco.com",
                UserName = "phphome",
                Application = applications.First(x => x.Id == EcohomeAppId),
                ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                EmailConfirmed = true,
                PhoneNumberConfirmed = false,
                TwoFactorEnabled = false,
                LockoutEnabled = false,
                AccessFailedCount = 0,
                EsignatureEnabled = true,
                PasswordHash = SecurityUtils.HashPassword("phphome"),//"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                //Password: 123456789
                SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                Company = "PHP Home Services",
                DisplayName = "PHP Home Services",
                AspireAccountId = "70214",
                AspireLogin = "phphome",
                AspirePassword = "phphome"
            };
            users.Add(newUser);

            //leave existing users data
            users.RemoveAll(u => context.Users.Any(dbu => dbu.UserName == u.UserName));
            context.Users.AddOrUpdate(u => u.UserName, users.ToArray());
        }

        private void SetTestEquipmentTypes(ApplicationDbContext context)
        {
            var equipmentTypes = new List<EquipmentType>
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
            //leave existing data
            equipmentTypes.RemoveAll(e => context.EquipmentTypes.Any(dbe => dbe.Type == e.Type));
            context.EquipmentTypes.AddOrUpdate(e => e.Type, equipmentTypes.ToArray());
        }

        private void SetTestProvinceTaxRates(ApplicationDbContext context)
        {
            //Obtained from http://www.retailcouncil.org/quickfacts/taxrates
            var taxRates = new List<ProvinceTaxRate>
            {
                new ProvinceTaxRate {Province = "AB", Rate = 5, Description = "GST"},
                new ProvinceTaxRate {Province = "BC", Rate = 12, Description = "GST + PST"},
                new ProvinceTaxRate {Province = "MB", Rate = 13, Description = "GST + PST"},
                new ProvinceTaxRate {Province = "NB", Rate = 15, Description = "HST"},
                new ProvinceTaxRate {Province = "NL", Rate = 15, Description = "HST"},
                new ProvinceTaxRate {Province = "NT", Rate = 5, Description = "GST"},
                new ProvinceTaxRate {Province = "NS", Rate = 15, Description = "HST"},
                new ProvinceTaxRate {Province = "NU", Rate = 5, Description = "GST"},
                new ProvinceTaxRate {Province = "ON", Rate = 13, Description = "HST"},
                new ProvinceTaxRate {Province = "PE", Rate = 15, Description = "HST"},
                new ProvinceTaxRate {Province = "QC", Rate = 14.975, Description = "GST + QST"},
                new ProvinceTaxRate {Province = "SK", Rate = 10, Description = "GST + PST"},
                new ProvinceTaxRate {Province = "YT", Rate = 5, Description = "GST"}
            };
            //leave existing data
            //taxRates.RemoveAll(t => context.ProvinceTaxRates.Any(dbt => dbt.Province == t.Province));
            context.ProvinceTaxRates.AddOrUpdate(t => t.Province, taxRates.ToArray());
        }

        private void SetDocumentTypes(ApplicationDbContext context)
        {
            var documentTypes = new List<DocumentType>
            {
                new DocumentType()  {Description = "Signed contract", Prefix = "SC_"},
                new DocumentType()  {Description = "Signed Installation certificate", Prefix = "SIC_"},
                new DocumentType()  {Description = "Invoice", Prefix = "INV_"},
                new DocumentType()  {Description = "Copy of Void Personal Cheque", Prefix = "VPC_"},
                new DocumentType()  {Description = "Extended Warranty Form", Prefix = "EWF_"},
                new DocumentType()  {Description = "Third party verification call", Prefix = "TPV_"},
                new DocumentType()  {Description = "Other", Prefix = ""},
            };
            //leave existing data
            documentTypes.RemoveAll(d => context.DocumentTypes.Any(dbd => dbd.Description == d.Description));
            context.DocumentTypes.AddOrUpdate(d => d.Description, documentTypes.ToArray());
        }

        private AgreementTemplate[] SetDocuSignTemplates(ApplicationDbContext context)
        {
           List<AgreementTemplate> templates = new List<AgreementTemplate>();
            
            var template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EcoSmart HVAC Rental",
                ExternalTemplateId = "96f6775e-a18a-466b-b275-a845d63c6f6c",//"a8c47648-542c-4edf-b222-3168d39d4d68",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecosmart"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);
                       
            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Canadian Home Efficiency HVAC",
                ExternalTemplateId = "d2310353-8088-4ba0-9ea3-18278e6f168a",//"b6f6aa88-d405-4921-85c2-e1a4bd2162cd",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("canadianhome"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EnerTech Home Services H.V.A.C OTHER RENTAL AGREEMENT",
                ExternalTemplateId = "37c64c0e-5de3-4e78-a931-683e3b735ec5",//"36301cc8-07b1-4205-a96e-e9e647e7e110",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("enertech"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EFFICIENCY STANDARDS - HVAC RENTAL",
                ExternalTemplateId = "ad0280c0-1312-4a29-96ac-ef6a69e29b98",//"567ece58-44ab-45f8-8085-6a6e68457e0e",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                EquipmentType = "ECO44",
                TemplateName = "EFFICIENCY STANDARDS - WATER SOFTENER RENTAL",
                ExternalTemplateId = "369af238-2db8-43e0-b1af-16d7377e5df5",//"78f231cf-6d08-4fdc-8eaa-f06c5552153c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            // EcoEnergy users           
            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Eco Energy Rental App 3-18-15",
                ExternalTemplateId = "0153d9ad-7d65-4c8b-9322-a594686529ba",//"c68e3bf5-b6c5-4291-9392-82102371948b",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "AB",
                TemplateName = "EEHS - Rental App - Alberta 2-22-16",
                ExternalTemplateId = "5a46958f-2697-4042-8e3b-b7de9bed3864", //"67b4cff0-d95c-43ed-9696-1b9c7fa2d1f3",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Apex Home Services Rental Agreement",
                ExternalTemplateId = "74b92c48-9b15-4bf3-9caf-0b5afdf8ba97",//"598be4b6-855b-4684-a0ee-fb5c83eb1eeb",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Apex"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "OSS RENTAL AGREEMENT - EGD 7-8-16",
                ExternalTemplateId = "a1abda2a-c1ef-46ff-b15c-2617b25e7013", //"a7ef2bce-abfb-4643-8133-884b19f0b354",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Ontario"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "OGSI RENTAL AGREEMENT - EGD 6-23-16",
                ExternalTemplateId = "2c252e19-8341-4ab2-8618-04bcf3d4ebfe", //"6af6000b-6079-4ffd-970c-41bfb1639e5c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Ontario Green"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "ELHC RENTAL AGREEMENT - EGD 5-11-2016",
                ExternalTemplateId = "5e362fbc-2ba0-43ed-882b-8ffe10f26379",//"dc11e414-b7c6-4f9a-bdaf-7e09c8c79f63",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("EcoLife"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "Smart Home Ontario Loan Agreement Nov 2016",
                ExternalDealerName = "smarthome",
                ExternalTemplateId = "294a0dfb-6b32-4c23-975f-449f78986f6a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("smarthome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("smarthome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("smarthome"))?.Id,
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "Smart Home HVAC agreement Nov 2016 M",
                ExternalDealerName = "smarthome",
                ExternalTemplateId = "a81ef5aa-d65b-43f0-86bf-7020f6c74e14",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("smarthome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("smarthome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("smarthome"))?.Id,
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome (ON) rental HVAC Other Equipment",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "b89a15e1-77e7-4506-83f6-be23e7272a21",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                //EquipmentType = "ECO11",
                AgreementType = AgreementType.RentalApplicationHwt,
                TemplateName = "EcoHome Generic Water Heater Agreement",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "7a543d1a-f581-4f93-9903-decc3db38a99",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome (ON) rental HVAC Other Equipment",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "b89a15e1-77e7-4506-83f6-be23e7272a21",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
            };
            templates.Add(template);

            templates.RemoveAll(t => context.AgreementTemplates.Any(at => at.TemplateName == t.TemplateName && at.DealerId == t.DealerId && at.AgreementType == t.AgreementType));
            //context.AgreementTemplates.AddOrUpdate(t => new { t.TemplateName, t.DealerId, t.AgreementType }, templates.ToArray());
            AddOrUpdate(context, t => new { t.TemplateName, t.DealerId, t.AgreementType }, templates.ToArray());

            return templates.ToArray();
        }

        private void SetPdfTemplates(ApplicationDbContext context, AgreementTemplate[] templates)
        {            
            templates.ForEach(t =>
            {
                try
                {
                    var dir = HostingEnvironment.MapPath("~/SeedData");
                    var path = Path.Combine(dir ?? "", t.TemplateName + ".pdf");
                    if (File.Exists(path))
                    {
                        var templt = context.AgreementTemplates.Local.FirstOrDefault(tmplt => tmplt.TemplateName == t.TemplateName);
                        if (templt != null)
                        {
                            templt.AgreementForm = File.ReadAllBytes(path);
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }        

        private void SetExistingPdfTemplates(ApplicationDbContext context)
        {
            context.AgreementTemplates.ForEach(t =>
            {
                try
                {
                    var dir = HostingEnvironment.MapPath("~/SeedData");
                    var path = Path.Combine(dir ?? "", t.TemplateName + ".pdf");
                    if (File.Exists(path))
                    {
                        t.AgreementForm = File.ReadAllBytes(path);                        
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }

        public static void AddOrUpdate<TEntity>(DbContext context, Expression<Func<TEntity, object>> identifiers, params TEntity[] entities) where TEntity : class
        {
            var primaryKeys = PrimaryKeys<TEntity>();
            var properties = Properties<TEntity>(identifiers);

            for (var i = 0; i < entities.Length; i++)
            {
                // build where condition for "identifiers"
                var parameter = Expression.Parameter(typeof(TEntity));
                var matches = properties.Select(p => Expression.Equal(
                    Expression.Property(parameter, p),
                    Expression.Constant(p.GetValue(entities[i]), p.PropertyType)));
                var match = Expression.Lambda<Func<TEntity, bool>>(
                    matches.Aggregate((p, q) => Expression.AndAlso(p, q)),
                    parameter);

                // match "identifiers" for current item
                var current = context.Set<TEntity>().SingleOrDefault(match);
                if (current != null)
                {
                    // update primary keys
                    foreach (var k in primaryKeys)
                        k.SetValue(entities[i], k.GetValue(current));

                    // update all the values
                    context.Entry(current).CurrentValues.SetValues(entities[i]);

                    // replace updated item
                    entities[i] = current;
                }
                else
                {
                    // add new item
                    entities[i] = context.Set<TEntity>().Add(entities[i]);
                }
            }
        }

        private static PropertyInfo[] PrimaryKeys<TEntity>() where TEntity : class
        {
            return typeof(TEntity).GetProperties()
                                  .Where(p => Attribute.IsDefined(p, typeof(KeyAttribute))
                                           || "Id".Equals(p.Name, StringComparison.Ordinal))
                                  .ToArray();
        }

        private static PropertyInfo[] Properties<TEntity>(Expression<Func<TEntity, object>> identifiers) where TEntity : class
        {
            // e => e.SomeValue
            var direct = identifiers.Body as MemberExpression;
            if (direct != null)
            {
                return new[] { (PropertyInfo)direct.Member };
            }

            // e => (object)e.SomeValue
            var convert = identifiers.Body as UnaryExpression;
            if (convert != null)
            {
                return new[] { (PropertyInfo)((MemberExpression)convert.Operand).Member };
            }

            // e => new { e.SomeValue, e.OtherValue }
            var multiple = identifiers.Body as NewExpression;
            if (multiple != null)
            {
                return multiple.Arguments
                               .Cast<MemberExpression>()
                               .Select(a => (PropertyInfo)a.Member)
                               .ToArray();
            }

            throw new NotSupportedException();
        }

    }
}
