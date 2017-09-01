using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Configuration;
using System.Web.Hosting;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Domain;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
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
        private readonly IAppConfiguration _configuration;

        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
            ContextKey = "DealnetPortal.DataAccess.ApplicationDbContext";
            _configuration = new Utilities.Configuration.AppConfiguration(WebConfigSections.AdditionalSections);
        }

        protected override void Seed(DealnetPortal.DataAccess.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            bool dataSeedEnabled = true;
            if (_configuration.GetSetting(WebConfigKeys.INITIAL_DATA_SEED_ENABLED_CONFIG_KEY) != null)
            {
                bool.TryParse(_configuration.GetSetting(WebConfigKeys.INITIAL_DATA_SEED_ENABLED_CONFIG_KEY),
                    out dataSeedEnabled);                
            }

            if (dataSeedEnabled)
            {
                var applications = SetApplications(context);
                SetRoles(context);
                SetTiers(context);
                SetServiceUsers(context, context.Applications.Local.ToArray());
                SetAspireTestUsers(context, context.Applications.Local.ToArray());
                SetTestEquipmentTypes(context);
                SetTestProvinceTaxRates(context);
                SetAspireStatuses(context);
                SetDocumentTypes(context);
                SetLanguages(context);
                var templates = SetDocuSignTemplates(context);
                SetInstallationCertificateTemplates(context, context.Applications.Local.ToArray());
                SetPdfTemplates(context, templates);                                
                SetSettingItems(context);
                SetUserSettings(context);                
                SetRateCards(context);
            }
            //read updated pdt templates anyway
            SetExistingPdfTemplates(context);
            //read daelers logos anyway
            SetUserLogos(context);
        }

        public void SetTiers(ApplicationDbContext context)
        {
            context.Tiers.AddOrUpdate(new Tier
            {
                Id = 1,
                Name = "Rate Card Tier 1"
            });

            context.Tiers.AddOrUpdate(new Tier
            {
                Id = 2,
                Name = "Rate Card Tier 2",

            });
        }

        private void SetRoles(ApplicationDbContext context)
        {
            var roles = Enum.GetValues(typeof(UserRole)).Cast<UserRole>().Select(r => new IdentityRole() {Name = r.ToString()});
            context.Roles.AddOrUpdate(r => r.Name, roles.ToArray());
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
        
        /// Set special Service Users to a DB
        private void SetServiceUsers(ApplicationDbContext context, Application[] applications)
        {
            var customerCreatorUserName = "CustomerCreator";            
            //Add customer creator to group
            //var appRoleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));            
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            if (userManager.FindByName(customerCreatorUserName) == null)
            {
                var customerCreator = new ApplicationUser()
                {
                    Email = "customerCreator@user.com",
                    UserName = customerCreatorUserName,
                    Application = applications.First(x => x.Id == EcohomeAppId),
                    ApplicationId = applications.First(x => x.Id == EcohomeAppId)?.Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    EsignatureEnabled = false,
                    //PasswordHash = "AAInS7oMLYVc0Z6tOXbu224LqdIGygS7kGnngFWX8jB4JHjRpZYSYwubaf3D6LknnA==",
                    //Password: 123_Qwe
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773"
                };
                var addResult = userManager.Create(customerCreator, "123_Qwe");
                if (addResult.Succeeded)
                {
                    userManager.AddToRole(customerCreator.Id, UserRole.CustomerCreator.ToString());
                }
            }
        }

        private void SetAspireTestUsers(ApplicationDbContext context, Application[] applications)
        {
            //Do not set users to a DB is DB is not empty
            if (!context.Users.Any())
            {
                List<ApplicationUser> users = new List<ApplicationUser>();

                //One Dealer users
                var onedealerUser = new ApplicationUser()
                {
                    Email = "onedealer@onedealer.com",
                    UserName = "onedealer",
                    Application = applications.First(x => x.Id == OdiAppId),
                    ApplicationId = applications.First(x => x.Id == OdiAppId)?.Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    EsignatureEnabled = true,
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    // "ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ODI",
                    DisplayName = "One Dealer",
                    AspireAccountId = string.Empty,
                    AspireLogin = "onedealer",
                };
                users.Add(onedealerUser);

                onedealerUser = new ApplicationUser()
                {
                    Email = "onedealer@onedealer.com",
                    UserName = "greenessential",
                    Application = applications.First(x => x.Id == OdiAppId),
                    ApplicationId = applications.First(x => x.Id == OdiAppId)?.Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    EsignatureEnabled = true,
                    PasswordHash = SecurityUtils.HashPassword("123456"),
                    // "ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "Green Essential Services",
                    DisplayName = "Green Essential Services",
                    AspireAccountId = string.Empty,
                    AspireLogin = "greenessential",
                };
                users.Add(onedealerUser);

                onedealerUser = new ApplicationUser()
                {
                    Email = "onedealer@onedealer.com",
                    UserName = "ohwater",
                    Application = applications.First(x => x.Id == OdiAppId),
                    ApplicationId = applications.First(x => x.Id == OdiAppId)?.Id,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = false,
                    AccessFailedCount = 0,
                    EsignatureEnabled = true,
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    // "ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "Ontario HVAC and Water",
                    DisplayName = "Ontario HVAC and Water",
                    AspireAccountId = string.Empty,
                    AspireLogin = "ohwater",
                };
                users.Add(onedealerUser);

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
                    PasswordHash = SecurityUtils.HashPassword("123456"),
                    // "ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Eco Smart Home Services",
                    AspireAccountId = "70017",
                    AspireLogin = "ecosmart",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Canadian Home Efficiency Services",
                    AspireAccountId = "70122",
                    AspireLogin = "canadianhome",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Enertech Home Services",
                    AspireAccountId = "70133",
                    AspireLogin = "enertech",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Efficiency Standard Home Services",
                    AspireAccountId = "70116",
                    AspireLogin = "efficiency",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("123456789"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Eco Energy Home Services",
                    AspireAccountId = "70015",
                    AspireLogin = "ecoenergy",
                    TierId = 1
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
                    TierId = 1
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
                    TierId = 1
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
                    TierId = 1
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
                    TierId = 1
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
                    TierId = 1
                };
                ecoenergyUser.SubDealers.Add(ecoenergySubUser);
                users.Add(ecoenergySubUser);
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
                    PasswordHash = SecurityUtils.HashPassword("password"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Smart Home",
                    AspireAccountId = "70101",
                    AspireLogin = "smarthome",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("dangelo"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "ECO",
                    DisplayName = "Eco Home",
                    AspireAccountId = "70073",
                    AspireLogin = "Dangelo",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("fahrhall"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "Farhall Mechanical",
                    DisplayName = "Farhall Mechanical",
                    AspireAccountId = "70266",
                    AspireLogin = "fahrhall",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("lifetimewater"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "Life-Time Water",
                    DisplayName = "Life-Time Water",
                    AspireAccountId = "70182",
                    AspireLogin = "lifetimewater",
                    TierId = 1
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
                    PasswordHash = SecurityUtils.HashPassword("phphome"),
                    //"ACQLO+Y4ju3euoQ4A1JEbrbGtHb8IOIDgMuTtHVMixjncpUi6OG227kzAL1sqEe5SQ==",
                    //Password: 123456789
                    SecurityStamp = "27a6bb1c-4737-4ab1-b0f8-ec3122ee2773",
                    Company = "PHP Home Services",
                    DisplayName = "PHP Home Services",
                    AspireAccountId = "70214",
                    AspireLogin = "phphome",
                    TierId = 1
                };
                users.Add(newUser);

                //leave existing users data
                users.RemoveAll(u => context.Users.Any(dbu => dbu.UserName == u.UserName));
                //add new to db
                context.Users.AddOrUpdate(u => u.UserName, users.ToArray());
            }
        }

        private void SetTestEquipmentTypes(ApplicationDbContext context)
        {
            //do not set equipments it DB is not empty
            if (!context.EquipmentTypes.Any())
            {
                var equipmentTypes = new List<EquipmentType>
                {
                    new EquipmentType
                    {
                        Description = "Air Conditioner",
                        DescriptionResource = "AirConditioner",
                        Type = "ECO1"
                    },
                    new EquipmentType {Description = "Boiler", DescriptionResource = "Boiler", Type = "ECO2"},
                    new EquipmentType {Description = "Doors", DescriptionResource = "Doors", Type = "ECO3"},
                    new EquipmentType {Description = "Fireplace", DescriptionResource = "Fireplace", Type = "ECO4"},
                    new EquipmentType {Description = "Furnace", DescriptionResource = "Furnace", Type = "ECO5"},
                    new EquipmentType {Description = "HWT", DescriptionResource = "Hwt", Type = "ECO6"},
                    new EquipmentType {Description = "Plumbing", DescriptionResource = "Plumbing", Type = "ECO7"},
                    new EquipmentType {Description = "Roofing", DescriptionResource = "Roofing", Type = "ECO9"},
                    new EquipmentType {Description = "Siding", DescriptionResource = "Siding", Type = "ECO10"},
                    new EquipmentType
                    {
                        Description = "Tankless Water Heater",
                        DescriptionResource = "TanklessWaterHeater",
                        Type = "ECO11"
                    },
                    new EquipmentType {Description = "Windows", DescriptionResource = "Windows", Type = "ECO13"},
                    new EquipmentType {Description = "Sunrooms", DescriptionResource = "Sunrooms", Type = "ECO38"},
                    new EquipmentType {Description = "Air Handler", DescriptionResource = "AirHandler", Type = "ECO40"},
                    new EquipmentType {Description = "Flooring", DescriptionResource = "Flooring", Type = "ECO42"},
                    new EquipmentType
                    {
                        Description = "Porch Enclosure",
                        DescriptionResource = "PorchEnclosure",
                        Type = "ECO43"
                    },
                    new EquipmentType
                    {
                        Description = "Water Treatment System",
                        DescriptionResource = "WaterTreatmentSystem",
                        Type = "ECO44"
                    },
                    new EquipmentType {Description = "Heat Pump", DescriptionResource = "HeatPump", Type = "ECO45"},
                    new EquipmentType {Description = "HRV", DescriptionResource = "Hrv", Type = "ECO46"},
                    new EquipmentType {Description = "Bathroom", DescriptionResource = "Bathroom", Type = "ECO47"},
                    new EquipmentType {Description = "Kitchen", DescriptionResource = "Kitchen", Type = "ECO48"},
                    new EquipmentType {Description = "Hepa System", DescriptionResource = "HepaSystem", Type = "ECO49"},
                    //new EquipmentType {Description = "Unknown", DescriptionResource = "Unknown", Type = "ECO50"},
                    //new EquipmentType {Description = "Security System", DescriptionResource = "SecuritySystem", Type = "ECO52"},
                    new EquipmentType
                    {
                        Description = "Basement Repair",
                        DescriptionResource = "BasementRepair",
                        Type = "ECO55"
                    },
                    new EquipmentType {Description = "Spa", DescriptionResource = "Spa", Type = "ECO58"},
                    new EquipmentType {Description = "Well pump", DescriptionResource = "WellPump", Type = "ECO59"},
                    new EquipmentType
                    {
                        Description = "Air Filtration",
                        DescriptionResource = "AirFiltration",
                        Type = "ECO23"
                    },
                    new EquipmentType {Description = "Hot Tub", DescriptionResource = "HotTub", Type = "ECO54"},
                    new EquipmentType
                    {
                        Description = "Vertical Fan/HRV Combo",
                        DescriptionResource = "VerticalFanHRVCombo",
                        Type = "ECO60"
                    }
                };
                //leave existing data
                equipmentTypes.RemoveAll(e => context.EquipmentTypes.Any(dbe => dbe.Type == e.Type));
                context.EquipmentTypes.AddOrUpdate(e => e.Type, equipmentTypes.ToArray());
            }
        }

        private void SetTestProvinceTaxRates(ApplicationDbContext context)
        {
            //Obtained from http://www.retailcouncil.org/quickfacts/taxrates
            var taxRates = new List<ProvinceTaxRate>
            {
                new ProvinceTaxRate {Province = "AB", Rate = 5, Description = "Gst"},
                new ProvinceTaxRate {Province = "BC", Rate = 12, Description = "GstPst"},
                new ProvinceTaxRate {Province = "MB", Rate = 13, Description = "GstPst"},
                new ProvinceTaxRate {Province = "NB", Rate = 15, Description = "Hst"},
                new ProvinceTaxRate {Province = "NL", Rate = 15, Description = "Hst"},
                new ProvinceTaxRate {Province = "NT", Rate = 5, Description = "Gst"},
                new ProvinceTaxRate {Province = "NS", Rate = 15, Description = "Hst"},
                new ProvinceTaxRate {Province = "NU", Rate = 5, Description = "Gst"},
                new ProvinceTaxRate {Province = "ON", Rate = 13, Description = "Hst"},
                new ProvinceTaxRate {Province = "PE", Rate = 15, Description = "Hst"},
                new ProvinceTaxRate {Province = "QC", Rate = 14.975, Description = "GstQst"},
                new ProvinceTaxRate {Province = "SK", Rate = 11, Description = "GstPst"},
                new ProvinceTaxRate {Province = "YT", Rate = 5, Description = "Gst"}
            };
            //leave existing data
            taxRates.RemoveAll(t => context.ProvinceTaxRates.Any(dbt => dbt.Province == t.Province));
            context.ProvinceTaxRates.AddOrUpdate(t => t.Province, taxRates.ToArray());
        }

        private void SetAspireStatuses(ApplicationDbContext context)
        {
            var statuses = new List<AspireStatus>
            {
                 new AspireStatus { Status = "Booked", Interpretation = AspireStatusInterpretation.SentToAudit },
                 new AspireStatus { Status = "Ready for Audit", Interpretation = AspireStatusInterpretation.SentToAudit }
            };
            //leave existing data
            statuses.RemoveAll(t => context.AspireStatuses.Any(dbt => dbt.Status == t.Status));
            context.AspireStatuses.AddOrUpdate(t => t.Status, statuses.ToArray());
        }

        private void SetDocumentTypes(ApplicationDbContext context)
        {
            var documentTypes = new List<DocumentType>
            {
                new DocumentType()  {Id = (int)DocumentTemplateType.SignedContract, Description = "Signed contract", DescriptionResource = "SignedContract", Prefix = "SC_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.SignedInstallationCertificate, Description = "Signed Installation certificate", DescriptionResource = "SignedInstallationCertificate", Prefix = "SIC_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.Invoice, Description = "Invoice", DescriptionResource = "Invoice", Prefix = "INV_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.VoidPersonalCheque, Description = "Copy of Void Personal Cheque", DescriptionResource = "VoidPersonalChequeCopy", Prefix = "VPC_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.ExtendedWarrantyForm, Description = "Extended Warranty Form", DescriptionResource = "ExtendedWarrantyForm", Prefix = "EWF_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.VerificationCall, Description = "Third party verification call", DescriptionResource = "ThirdPartyVerificationCall", Prefix = "TPV_"},
                new DocumentType()  {Id = (int)DocumentTemplateType.Other, Description = "Other", DescriptionResource = "Other", Prefix = ""},
            };
            //leave existing data
            documentTypes.RemoveAll(d => context.DocumentTypes.Any(dbd => dbd.Description == d.Description));
            context.DocumentTypes.AddOrUpdate(d => d.Description, documentTypes.ToArray());
        }

        private void SetLanguages(ApplicationDbContext context)
        {
            var languages = new List<Language>
            {
                new Language() {Id = (int)LanguageCode.English, Code = "en", Name = "English"},
                new Language() {Id = (int)LanguageCode.French, Code = "fr", Name = "French"}
            };
            context.Languages.AddOrUpdate(l => l.Code, languages.ToArray());
        }

        private AgreementTemplate[] SetDocuSignTemplates(ApplicationDbContext context)
        {
           List<AgreementTemplate> templates = new List<AgreementTemplate>();
            
            var template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoSmart HVAC Rental",
                ExternalTemplateId = "96f6775e-a18a-466b-b275-a845d63c6f6c",//"a8c47648-542c-4edf-b222-3168d39d4d68",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecosmart"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecosmart"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);
                       
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "Canadian Home Efficiency HVAC",
                ExternalTemplateId = "d2310353-8088-4ba0-9ea3-18278e6f168a",//"b6f6aa88-d405-4921-85c2-e1a4bd2162cd",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("canadianhome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("canadianhome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EnerTech Home Services H.V.A.C OTHER RENTAL AGREEMENT",
                ExternalTemplateId = "37c64c0e-5de3-4e78-a931-683e3b735ec5",//"36301cc8-07b1-4205-a96e-e9e647e7e110",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("enertech"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("enertech"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EFFICIENCY STANDARDS - HVAC RENTAL",
                ExternalTemplateId = "ad0280c0-1312-4a29-96ac-ef6a69e29b98",//"567ece58-44ab-45f8-8085-6a6e68457e0e",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("efficiency"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            // EcoEnergy users           
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "Eco Energy Rental App 3-18-15",
                ExternalTemplateId = "0153d9ad-7d65-4c8b-9322-a594686529ba",//"c68e3bf5-b6c5-4291-9392-82102371948b",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);
            template = new AgreementTemplate()
            {
                AgreementType = AgreementType.RentalApplicationHwt,
                TemplateName = "Eco Energy Rental HWT App 4-8-15",
                ExternalTemplateId = "8d0ad210-99a3-41b7-99f8-8e2f8ec79088",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "AB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EEHS - Rental App - Alberta 2-22-16",
                ExternalTemplateId = "5a46958f-2697-4042-8e3b-b7de9bed3864", //"67b4cff0-d95c-43ed-9696-1b9c7fa2d1f3",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("ecoenergy"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "Apex Home Services Rental Agreement",
                ExternalTemplateId = "74b92c48-9b15-4bf3-9caf-0b5afdf8ba97",//"598be4b6-855b-4684-a0ee-fb5c83eb1eeb",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Apex"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Apex"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "OSS RENTAL AGREEMENT - EGD 7-8-16",
                ExternalTemplateId = "a1abda2a-c1ef-46ff-b15c-2617b25e7013", //"a7ef2bce-abfb-4643-8133-884b19f0b354",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Ontario"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "OGSI RENTAL AGREEMENT - EGD 6-23-16",
                ExternalTemplateId = "2c252e19-8341-4ab2-8618-04bcf3d4ebfe", //"6af6000b-6079-4ffd-970c-41bfb1639e5c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Ontario Green"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Ontario Green"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            //context.AgreementTemplates.Add(template);
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "ELHC RENTAL AGREEMENT - EGD 5-11-2016",
                ExternalTemplateId = "5e362fbc-2ba0-43ed-882b-8ffe10f26379",//"dc11e414-b7c6-4f9a-bdaf-7e09c8c79f63",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("EcoLife"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("EcoLife"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "AB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome  (AB)  Financial Credit Application and Loan Agreement (Alberta)",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "d9979df7-bada-4bc4-a7b3-21e3a35f8425",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "BC",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (BC)  Financial EHF Credit Application and Loan Agreement Aug 20",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "952884d2-e64d-43e2-a5db-a10d2707d0d8",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "AB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome  (AB)  Financial Credit Application and Loan Agreement (Alberta)",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "d9979df7-bada-4bc4-a7b3-21e3a35f8425",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "BC",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (BC)  Financial EHF Credit Application and Loan Agreement Aug 20",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "952884d2-e64d-43e2-a5db-a10d2707d0d8",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (ON) loan agreement August 2016",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "687661a4-0b53-4816-ac55-9523b6f255f5",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "BC",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (BC)  Financial EHF Credit Application and Loan Agreement Aug 20",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "952884d2-e64d-43e2-a5db-a10d2707d0d8",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "AB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome  (AB)  Financial Credit Application and Loan Agreement (Alberta)",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "d9979df7-bada-4bc4-a7b3-21e3a35f8425",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
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
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome HVAC Other Equipment - GENERIC 11.99% (ON) NOV M 2016",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "17b10776-d009-4835-be8d-54dbca5352a7",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "AB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome Rental Agreement - Alberta M 11.99% - NOV 2016",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "443e2cc5-ec12-4153-b111-f90c70997f85",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                State = "BC",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (BC)  Financial EHF Credit Application and Loan Agreement Aug 20",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "952884d2-e64d-43e2-a5db-a10d2707d0d8",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            // New Templates

            #region EcoHome (MB) Financial  Loan Agreement - MB Aug 2016
            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (MB) Financial  Loan Agreement - MB Aug 2016",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "8f5686e3-8a2a-4fa4-9243-7f52527f67ac",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (MB) Financial  Loan Agreement - MB Aug 2016",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "8f5686e3-8a2a-4fa4-9243-7f52527f67ac",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (MB) Financial  Loan Agreement - MB Aug 2016",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "8f5686e3-8a2a-4fa4-9243-7f52527f67ac",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (MB) Financial  Loan Agreement - MB Aug 2016",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "8f5686e3-8a2a-4fa4-9243-7f52527f67ac",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            #endregion

            #region EcoHome (SK) Credit Application and Loan Agreement (Saskatchewan) Aug 2016
            template = new AgreementTemplate()
            {
                State = "SK",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (SK) Credit Application and Loan Agreement (Saskatchewan) Aug 2016",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "df2b6600-2492-4fa7-8e4c-f9e4edf0b620",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "SK",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (SK) Credit Application and Loan Agreement (Saskatchewan) Aug 2016",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "df2b6600-2492-4fa7-8e4c-f9e4edf0b620",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "SK",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (SK) Credit Application and Loan Agreement (Saskatchewan) Aug 2016",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "df2b6600-2492-4fa7-8e4c-f9e4edf0b620",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "SK",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome (SK) Credit Application and Loan Agreement (Saskatchewan) Aug 2016",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "df2b6600-2492-4fa7-8e4c-f9e4edf0b620",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);
            #endregion

            #region EcoHome Credit Application and Loan Agreement  (New Brunswick) Nov 2016 M
            template = new AgreementTemplate()
            {
                State = "NB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome Credit Application and Loan Agreement  (New Brunswick) Nov 2016 M",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "1327014e-97a1-4508-9214-c19e0460ac0c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "NB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome Credit Application and Loan Agreement  (New Brunswick) Nov 2016 M",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "1327014e-97a1-4508-9214-c19e0460ac0c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "NB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome Credit Application and Loan Agreement  (New Brunswick) Nov 2016 M",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "1327014e-97a1-4508-9214-c19e0460ac0c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "NB",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "EcoHome Credit Application and Loan Agreement  (New Brunswick) Nov 2016 M",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "1327014e-97a1-4508-9214-c19e0460ac0c",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            #endregion

            #region EcoHome Rental Program Agreement (MB) 11.99% M Nov 2016 
            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome Rental Program Agreement (MB) 11.99% M Nov 2016 ",
                ExternalDealerName = "fahrhall",
                ExternalTemplateId = "485094e9-7b66-4a26-a756-7e2d8e35641a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("fahrhall"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome Rental Program Agreement (MB) 11.99% M Nov 2016 ",
                ExternalDealerName = "Dangelo",
                ExternalTemplateId = "485094e9-7b66-4a26-a756-7e2d8e35641a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("Dangelo"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome Rental Program Agreement (MB) 11.99% M Nov 2016 ",
                ExternalDealerName = "lifetimewater",
                ExternalTemplateId = "485094e9-7b66-4a26-a756-7e2d8e35641a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("lifetimewater"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "EcoHome Rental Program Agreement (MB) 11.99% M Nov 2016 ",
                ExternalDealerName = "phphome",
                ExternalTemplateId = "485094e9-7b66-4a26-a756-7e2d8e35641a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("phphome"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };
            templates.Add(template);

            #endregion

            #region One Dealer Generic Consumer Initiated Water Heater Agreement Nov 2016 M

            template = new AgreementTemplate()
            {
                AgreementType = AgreementType.RentalApplicationHwt,
                TemplateName = "One Dealer Generic Consumer Initiated Water Heater Agreement Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "f1dcbf9d-134a-4708-b590-eb45617d5816",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer Generic Water Heater Agreement (SF) Nov 2016 M

            template = new AgreementTemplate()
            {
                State = "SF",
                AgreementType = AgreementType.RentalApplicationHwt,
                TemplateName = "One Dealer Generic Water Heater Agreement (SF) Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "f8d76adf-0f27-442a-a775-43d1f830c0c8",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer Generic Water Heater Agreement Nov 2016 M

            template = new AgreementTemplate()
            {
                AgreementType = AgreementType.RentalApplicationHwt,
                TemplateName = "One Dealer Generic Water Heater Agreement Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "f1dcbf9d-134a-4708-b590-eb45617d5816",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer HVAC Other Equipment - GENERIC ON Nov 2016 M

            template = new AgreementTemplate()
            {
                State = "ON",
                AgreementType = AgreementType.LoanApplication,
                TemplateName = "One Dealer HVAC Other Equipment - GENERIC ON Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "ebe0f469-8386-4888-9009-3b457479489a",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer Rental Agreement - Alberta (OD) - Nov 2016 M

            template = new AgreementTemplate()
            {
                State = "OD",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "One Dealer Rental Agreement - Alberta (OD) - Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "2e9001b3-ff94-44a0-a27c-54f6fb533688",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer Rental Agreement - Saskatchewan M - Nov 2016

            template = new AgreementTemplate()
            {
                State = "SK",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "One Dealer Rental Agreement - Saskatchewan M - Nov 2016",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "c7e3750a-b24c-4860-afd7-8f1bf5978546",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

            #region One Dealer Rental Program Agreement (MB) Nov 2016 M

            template = new AgreementTemplate()
            {
                State = "MB",
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "One Dealer Rental Program Agreement (MB) Nov 2016 M",
                ExternalDealerName = "onedealer",
                ExternalTemplateId = "e473e31a-7b75-4578-8364-861fa827e073",
                Dealer = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer")),
                DealerId = context.Users.Local.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id ?? context.Users.FirstOrDefault(u => u.UserName.Contains("onedealer"))?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedContract
            };

            templates.Add(template);

            #endregion

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
                    var seedDataFolder = _configuration.GetSetting(WebConfigKeys.AGREEMENT_TEMPLATE_FOLDER_CONFIG_KEY);
                    var dir = HostingEnvironment.MapPath($"~/{seedDataFolder}");
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

        private void SetInstallationCertificateTemplates(ApplicationDbContext context, Application[] applications)
        {
            List<AgreementTemplate> templates = new List<AgreementTemplate>();

            var template = new AgreementTemplate()
            {
                AgreementType = AgreementType.RentalApplication,
                TemplateName = "ONE DEALER Completion Certificate - Rental",
                Application = applications.FirstOrDefault(x => x.Id == OdiAppId),
                ApplicationId = applications.FirstOrDefault(x => x.Id == OdiAppId)?.Id,
                DocumentTypeId = (int)DocumentTemplateType.SignedInstallationCertificate
            };
            templates.Add(template);

            template = new AgreementTemplate()
            {
                TemplateName = "EcoHome Completion Certificate - Rentals",
                Application = applications.FirstOrDefault(x => x.Id == EcohomeAppId),
                ApplicationId = applications.FirstOrDefault(x => x.Id == EcohomeAppId)?.Id,
                AgreementType = AgreementType.RentalApplication,
                DocumentTypeId = (int)DocumentTemplateType.SignedInstallationCertificate
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                TemplateName = "EcoHome Completion Certificate - Rentals",
                Application = applications.FirstOrDefault(x => x.Id == EcohomeAppId),
                ApplicationId = applications.FirstOrDefault(x => x.Id == EcohomeAppId)?.Id,
                AgreementType = AgreementType.RentalApplicationHwt,
                DocumentTypeId = (int)DocumentTemplateType.SignedInstallationCertificate
            };
            templates.Add(template);
            template = new AgreementTemplate()
            {
                TemplateName = "EcoHome Certificate of Completion - Loans",
                Application = applications.FirstOrDefault(x => x.Id == EcohomeAppId),
                ApplicationId = applications.FirstOrDefault(x => x.Id == EcohomeAppId)?.Id,
                AgreementType = AgreementType.LoanApplication,
                DocumentTypeId = (int)DocumentTemplateType.SignedInstallationCertificate
            };
            templates.Add(template);

            templates.RemoveAll(t => context.AgreementTemplates.Any(at => at.TemplateName == t.TemplateName && at.DealerId == t.DealerId && at.AgreementType == t.AgreementType && at.ApplicationId == t.ApplicationId));
            AddOrUpdate(context, t => new { t.TemplateName, t.DealerId, t.AgreementType }, templates.ToArray());
        }

        private void SetExistingPdfTemplates(ApplicationDbContext context)
        {
            context.AgreementTemplates.ForEach(t =>
            {
                try
                {
                    var seedDataFolder = _configuration.GetSetting(WebConfigKeys.AGREEMENT_TEMPLATE_FOLDER_CONFIG_KEY);
                    var dir = HostingEnvironment.MapPath($"~/{seedDataFolder}");
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

        private void SetSettingItems(ApplicationDbContext context)
        {
            var settingItems = new List<SettingItem>
            {
                new SettingItem()
                {
                    Name = "@navbar-header",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@logo-bg-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@logo-width",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@logo-height",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@navbar-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@nav-item-active-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@nav-item-active-box-shadow",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-shadow",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-disabled-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-disabled-shadow",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-hover-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-hover-shadow",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-active-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@btn-success-active-shadow",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-success-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-success-border-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-success-icon-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-info-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-info-border-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@well-info-icon-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@info-link-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@info-link-hover-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@info-link-disable-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@button-link-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@button-link-hover-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@button-link-disabled-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@button-link-active-color",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = "@table-tr-hover-bg",
                    SettingType = SettingType.StringValue
                },
                new SettingItem()
                {
                    Name = SettingType.LogoImage.ToString(),
                    SettingType = SettingType.LogoImage
                },
                new SettingItem()
                {
                    Name = SettingType.LogoImage2X.ToString(),
                    SettingType = SettingType.LogoImage2X
                },
                new SettingItem()
                {
                    Name = SettingType.LogoImage3X.ToString(),
                    SettingType = SettingType.LogoImage3X
                },
                new SettingItem()
                {
                    Name = SettingType.Favicon.ToString(),
                    SettingType = SettingType.Favicon
                }
            };
            context.SettingItems.AddOrUpdate(d => new { d.Name, d.SettingType }, settingItems.ToArray());
        }

        private void SetUserSettings(ApplicationDbContext context)
        {
            var ecoenergySettings = new Dictionary<string, string>()
            {
                { "@navbar-header", "#0b749a"},
                { "@logo-bg-color", "#006990"},
                { "@logo-width", "120px"},
                { "@logo-height", "24px"},
                { "@navbar-bg", "#208bb2"},
                { "@nav-item-active-bg", "#0e79a0"},
                { "@nav-item-active-box-shadow", "inset 0 -1px 0 0 rgba(255, 255, 255, 0.27)"},
                { "@btn-success-bg", "#009071"},
                { "@btn-success-shadow", "0 1px 0 0 #006953"},
                { "@btn-success-disabled-bg", "#009071"},
                { "@btn-success-disabled-shadow", "0 1px 0 0 #006953"},
                { "@btn-success-hover-bg", "#007a60"},
                { "@btn-success-hover-shadow", "0 1px 0 0 #00634e"},
                { "@btn-success-active-bg", "#007051"},
                { "@btn-success-active-shadow", "0 1px 0 0 #007051"},
                { "@well-success-bg", "#f1fffc"},
                { "@well-success-border-color", "rgba(1, 171, 135, 0.44)"},
                { "@well-success-icon-color", "#009071"},
                { "@well-info-bg", "rgba(48, 199, 255, 0.1)"},
                { "@well-info-border-color", "rgba(32, 139, 178, 0.34)"},
                { "@well-info-icon-color", "#208bb2"},
                { "@info-link-color", "#208bb2"},
                { "@info-link-hover-color", "#0f79a0"},
                { "@info-link-disable-color", "#63d0f8"},
                { "@button-link-color", "#008f71"},
                { "@button-link-hover-color", "#007a60"},
                { "@button-link-disabled-color", "#4db19c"},
                { "@button-link-active-color", "#007051"},
                { "@table-tr-hover-bg", "rgba(0, 144, 113, 0.05)" },
            };
            SetDealerStringSettings(context, "ecoenergy", ecoenergySettings);
            var smarthomeSettings = new Dictionary<string, string>()
            {
                { "@navbar-header", "#29559f"},
                { "@logo-bg-color", "rgba(6, 2, 34, 0.2)"},
                { "@logo-width", "117px"},
                { "@logo-height", "31px"},
                { "@navbar-bg", "#4470ba"},
                { "@nav-item-active-bg", "#3362b1"},
                { "@nav-item-active-box-shadow", "inset 0 -1px 0 0 rgba(255, 255, 255, 0.27)"},
                { "@btn-success-bg", "#00a3e6"},
                { "@btn-success-shadow", "0 1px 0 0 #007fb3"},
                { "@btn-success-disabled-bg", "#52c9fa"},
                { "@btn-success-disabled-shadow", "0 1px 0 0 #00a6ea"},
                { "@btn-success-hover-bg", "#0092cf"},
                { "@btn-success-hover-shadow", "0 1px 0 0 #0071a0"},
                { "@btn-success-active-bg", "#0075a5"},
                { "@btn-success-active-shadow", "0 1px 0 0 #005172"},
                { "@well-success-bg", "rgba(142, 208, 30, 0.07)"},
                { "@well-success-border-color", "#8ed01e"},
                { "@well-success-icon-color", "#8ed01e"},                
                { "@well-info-icon-color", "#00a3e6"},
                { "@info-link-color", "#76bb00"},
                { "@info-link-hover-color", "#6aa800"},
                { "@info-link-disable-color", "rgba(150, 220, 30, 0.45)"},
                { "@button-link-color", "#00a3e6"},
                { "@button-link-hover-color", "#0092ce"},
                { "@button-link-disabled-color", "#87dafc"},
                { "@button-link-active-color", "#0075a5"},
                { "@table-tr-hover-bg", "rgba(0, 163, 230, 0.05)" },
            };
            SetDealerStringSettings(context, "smarthome", smarthomeSettings);
            var lifetimewaterSettings = new Dictionary<string, string>()
            {
                { "@navbar-header", "#0688d8"},
                { "@logo-bg-color", "#007dc9"},
                { "@logo-width", "100px"},
                { "@logo-height", "29px"},
                { "@navbar-bg", "#24a1df"},
                { "@nav-item-active-bg", "#078fd2"},
                { "@nav-item-active-box-shadow", "inset 0 -1px 0 0 rgba(255, 255, 255, 0.27)"},
                { "@btn-success-bg", "#76c900"},
                { "@btn-success-shadow", "0 1px 0 0 #6bb500"},
                { "@btn-success-disabled-bg", "#76c900"},
                { "@btn-success-hover-bg", "#6ebb00"},
                { "@btn-success-hover-shadow", "0 1px 0 0 #61a500"},
                { "@btn-success-active-bg", "#62a700"},
                { "@btn-success-active-shadow", "0 1px 0 0 #599700"},
                { "@well-success-icon-color", "#76c900"},
                { "@table-tr-hover-bg", "rgba(118, 201, 0, 0.05)" },
            };
            SetDealerStringSettings(context, "lifetimewater", lifetimewaterSettings);
        }

        private void SetDealerStringSettings(ApplicationDbContext context, string userName, Dictionary<string, string> values)
        {
            var user = context.Users.Include(u => u.Settings).FirstOrDefault(u => u.UserName == userName) ?? context.Users.Local.FirstOrDefault(u => u.UserName == userName);
            if (user != null)
            {
                var userSetting = user.Settings;
                if (userSetting == null)
                {
                    userSetting = new UserSettings();
                    context.UserSettings.Add(userSetting);
                    user.Settings = userSetting;
                }               
                var valuesForAdd = values.Where(ns => userSetting.SettingValues.All(us => us.Item?.Name != ns.Key));
                valuesForAdd.ForEach(ns => userSetting.SettingValues.Add(new SettingValue()
                {
                    UserSettings = userSetting,
                    Item = context.SettingItems.Local.FirstOrDefault(u => u.Name == ns.Key),
                    StringValue = ns.Value
                }));
            }
        }

        private void SetUserLogos(ApplicationDbContext context)
        {
            context.Users.Include(u => u.Settings).ForEach(u =>
            {
                try
                {
                    var seedDataFolder = _configuration.GetSetting(WebConfigKeys.AGREEMENT_TEMPLATE_FOLDER_CONFIG_KEY);
                    var dir = HostingEnvironment.MapPath($"~/{seedDataFolder}") ?? "";

                    var files = Directory.GetFiles(dir, $"{u.UserName}*.*");
                    if (files.Any())
                    {                        
                        Enum.GetNames(typeof(SettingType)).Except(new string[] { SettingType.StringValue.ToString() }).ForEach(st =>
                        {                            
                            var filePath = files.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f)?.ToLowerInvariant().EndsWith(st.ToLowerInvariant()) ?? false);
                            if (!string.IsNullOrEmpty(filePath))
                            {
                                var logo =
                                    u.Settings?.SettingValues.FirstOrDefault(sv => sv.Item?.SettingType.ToString() == st);
                                if (logo == null)
                                {
                                    if (u.Settings == null)
                                    {
                                        var userSettings = new UserSettings();
                                        context.UserSettings.Add(userSettings);
                                        u.Settings = userSettings;
                                    }
                                    logo = new SettingValue()
                                    {
                                        UserSettings = u.Settings,
                                        Item =
                                            context.SettingItems.Local.FirstOrDefault(si => si.SettingType.ToString() == st),                                        
                                    };
                                    u.Settings?.SettingValues.Add(logo);
                                }
                                logo.BinaryValue = File.ReadAllBytes(filePath);
                            }
                        });
                    }
                }
                catch
                {
                    // ignored
                }
            });
        }

        private void SetRateCards(ApplicationDbContext context)
        {
            #region RateCards - Fixed Rate 1000 - 4999.99 Tier 1
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 1,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 2,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 3,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 4,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 5000 - 9999.99 Tier 1
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 5,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 6,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 7,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 8,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 9,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 10000 - 19999.99 Tier 1
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 10,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 11,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 12,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 13,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 14,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 20000 - 50000 Tier 1
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 15,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 16,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 17,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 18,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 19,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - NoInterest 1000 - 50000 Tier 1

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 20,
                LoanValueFrom = 1000,
                LoanValueTo = 50000,
                CustomerRate = 0,
                DealerCost = 8.25,
                AdminFee = 39.95,
                LoanTerm = 24,
                AmortizationTerm = 24,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.NoInterest,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 21,
                LoanValueFrom = 1000,
                LoanValueTo = 50000,
                CustomerRate = 0,
                DealerCost = 11.8,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.NoInterest,
                IsPromo = false
            });

            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 1 Defferal Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 22,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 23,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 24,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 25,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 1 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 26,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 27,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 28,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 29,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 30,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 1 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 31,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 32,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 33,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 34,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 35,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 1 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 36,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 37,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 38,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 39,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 40,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 2.2,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 1 Defferal Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 41,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 42,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 43,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 44,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 1 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 45,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 46,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 47,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 48,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 49,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 1 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 50,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 51,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 52,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 53,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 54,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 1 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 55,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 56,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 57,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 58,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 59,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 1 Defferal Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 60,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 61,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 62,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 63,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 1 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 64,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 65,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 66,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 67,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 68,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 8.74,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 1 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 69,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 70,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 71,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 72,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 73,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 7.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 1 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 74,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 75,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 76,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 77,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 78,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 6.99,
                DealerCost = 9,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 1000 - 4999.99 Tier 2
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 79,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 80,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 81,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 82,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 5000 - 9999.99 Tier 2
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 83,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 84,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 85,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 86,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 87,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 10000 - 19999.99 Tier 2
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 88,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 89,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 90,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 91,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 92,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - Fixed Rate 20000 - 50000 Tier 2
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 93,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 94,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 95,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 96,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 97,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 0,
                AdminFee = 39.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.FixedRate,
                IsPromo = false
            });
            #endregion

            #region RateCards - NoInterest 1000 - 50000 Tier 2

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 98,
                LoanValueFrom = 1000,
                LoanValueTo = 50000,
                CustomerRate = 0,
                DealerCost = 8.6,
                AdminFee = 39.95,
                LoanTerm = 24,
                AmortizationTerm = 24,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.NoInterest,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 99,
                LoanValueFrom = 1000,
                LoanValueTo = 50000,
                CustomerRate = 0,
                DealerCost = 12.3,
                AdminFee = 39.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 0,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.NoInterest,
                IsPromo = false
            });

            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 2 Defferal Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 100,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 101,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 102,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 103,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 2 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 104,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 105,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 106,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 107,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 108,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 2 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 109,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 110,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 111,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 112,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 113,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 2 Deferral Period 3
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 114,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 115,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 116,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 117,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 118,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 2.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 2 Defferal Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 119,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 120,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 121,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 3,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 122,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 2 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 123,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 124,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 125,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 126,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 127,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 2 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 128,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 129,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 130,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 131,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 132,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 2 Deferral Period 6
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 133,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 134,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 135,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 136,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 137,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 1000 - 4999.99 Tier 2 Defferal Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 138,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 139,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 140,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 141,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 5000 - 9999.99 Tier 2 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 142,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 143,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 144,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 145,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 146,
                LoanValueFrom = 5000,
                LoanValueTo = 9999.99,
                CustomerRate = 9.74,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 10000 - 19999.99 Tier 2 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 147,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 148,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 149,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 150,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 151,
                LoanValueFrom = 10000,
                LoanValueTo = 19999.99,
                CustomerRate = 8.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            #region RateCards - Deferral 20000 - 50000 Tier 2 Deferral Period 12
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 152,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 36,
                AmortizationTerm = 36,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 153,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 48,
                AmortizationTerm = 48,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 154,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 155,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 120,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 156,
                LoanValueFrom = 20000,
                LoanValueTo = 50000,
                CustomerRate = 7.99,
                DealerCost = 9.7,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 180,
                DeferralPeriod = 12,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });
            #endregion

            //Missed rate cards
            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 157,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 10.99,
                DealerCost = 4.5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 1,
                CardType = RateCardType.Deferral,
                IsPromo = false
            });

            context.RateCards.AddOrUpdate(new RateCard
            {
                Id = 158,
                LoanValueFrom = 1000,
                LoanValueTo = 4999.99,
                CustomerRate = 11.99,
                DealerCost = 5,
                AdminFee = 59.95,
                LoanTerm = 60,
                AmortizationTerm = 60,
                DeferralPeriod = 6,
                ValidFrom = null,
                ValidTo = null,
                TierId = 2,
                CardType = RateCardType.Deferral,
                IsPromo = false
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
