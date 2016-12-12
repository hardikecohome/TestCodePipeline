using System.Collections.Generic;
using System.IO;
using System.Web.Hosting;
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
            ContextKey = "DealnetPortal.DataAccess.ApplicationDbContext";
        }

        protected override void Seed(DealnetPortal.DataAccess.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.
            var applications = SetApplications(context);
            SetTestUsers(context, applications);
            SetAspireTestUsers(context, applications);
            SetTestEquipmentTypes(context);
            SetTestProvinceTaxRates(context);
            SetDocumentTypes(context);
            var templates = SetDocuSignTemplates(context);
            SetPdfTemplates(context, templates);
            var seedNames = templates.Select(at => at.TemplateName).ToArray();
            var dbTemplateNames =
                context.AgreementTemplates.Select(at => at.TemplateName).Where(tn => seedNames.All(t => t != tn)).ToArray();           
            SetExistingPdfTemplates(context, dbTemplateNames);
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
                new ProvinceTaxRate {Province = "AB", Rate = 5},
                new ProvinceTaxRate {Province = "BC", Rate = 12},
                new ProvinceTaxRate {Province = "MB", Rate = 13},
                new ProvinceTaxRate {Province = "NB", Rate = 13},
                new ProvinceTaxRate {Province = "NL", Rate = 13},
                new ProvinceTaxRate {Province = "NT", Rate = 5},
                new ProvinceTaxRate {Province = "NS", Rate = 15},
                new ProvinceTaxRate {Province = "NU", Rate = 5},
                new ProvinceTaxRate {Province = "ON", Rate = 13},
                new ProvinceTaxRate {Province = "PE", Rate = 5},
                new ProvinceTaxRate {Province = "QC", Rate = 14.975},
                new ProvinceTaxRate {Province = "SK", Rate = 10},
                new ProvinceTaxRate {Province = "YT", Rate = 5}
            };
            //leave existing data
            taxRates.RemoveAll(t => context.ProvinceTaxRates.Any(dbt => dbt.Province == t.Province));
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
                ExternalTemplateId = "a8c47648-542c-4edf-b222-3168d39d4d68",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);
                       
            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Canadian Home Efficiency HVAC",
                ExternalTemplateId = "b6f6aa88-d405-4921-85c2-e1a4bd2162cd",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EnerTech Home Services H.V.A.C OTHER RENTAL AGREEMENT",
                ExternalTemplateId = "36301cc8-07b1-4205-a96e-e9e647e7e110",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "EFFICIENCY STANDARDS - HVAC RENTAL",
                ExternalTemplateId = "567ece58-44ab-45f8-8085-6a6e68457e0e",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                EquipmentType = "ECO44",
                TemplateName = "EFFICIENCY STANDARDS - WATER SOFTENER RENTAL",
                ExternalTemplateId = "78f231cf-6d08-4fdc-8eaa-f06c5552153c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            // EcoEnergy users           
            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Eco Energy Rental App 3-18-15",
                ExternalTemplateId = "c68e3bf5-b6c5-4291-9392-82102371948b",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "AB",
                TemplateName = "EEHS - Rental App - Alberta 2-22-16",
                ExternalTemplateId = "67b4cff0-d95c-43ed-9696-1b9c7fa2d1f3",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "Apex Home Services Rental Agreement",
                ExternalTemplateId = "598be4b6-855b-4684-a0ee-fb5c83eb1eeb",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "OSS RENTAL AGREEMENT - EGD 7-8-16",
                ExternalTemplateId = "a7ef2bce-abfb-4643-8133-884b19f0b354",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "OGSI RENTAL AGREEMENT - EGD 6-23-16",
                ExternalTemplateId = "6af6000b-6079-4ffd-970c-41bfb1639e5c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                TemplateName = "ELHC RENTAL AGREEMENT - EGD 5-11-2016",
                ExternalTemplateId = "dc11e414-b7c6-4f9a-bdaf-7e09c8c79f63",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife"))?.Id,
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            //leave existing forms data
            //templates.ForEach(t =>
            //{
            //    var tmpl = context.AgreementTemplates.FirstOrDefault(dbt => dbt.TemplateName == t.TemplateName);
            //    if (tmpl != null)
            //    {
            //        templates.Remove(t);
            //    }                
            //});

            templates.RemoveAll(t => context.AgreementTemplates.Any(at => at.TemplateName == t.TemplateName));

            context.AgreementTemplates.AddOrUpdate(t => t.TemplateName, templates.ToArray());

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

        private void SetExistingPdfTemplates(ApplicationDbContext context, string[] templates)
        {
            templates.ForEach(t =>
            {
                try
                {
                    var dir = HostingEnvironment.MapPath("~/SeedData");
                    var path = Path.Combine(dir ?? "", t + ".pdf");
                    if (File.Exists(path))
                    {
                        var templt = context.AgreementTemplates.FirstOrDefault(tmplt => tmplt.TemplateName == t);
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
    }
}
