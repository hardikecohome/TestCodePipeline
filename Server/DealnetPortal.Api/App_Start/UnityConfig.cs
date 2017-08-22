using System.Configuration;
using System.Web.Hosting;
using Microsoft.Practices.Unity;
using System.Web.Http;
using DealnetPortal.Api.BackgroundScheduler;
using DealnetPortal.Api.Common.Constants;
using DealnetPortal.Api.Controllers;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.Api.Integration.Services.Signature;
using DealnetPortal.Aspire.Integration.ServiceAgents;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;
using DealnetPortal.Utilities.Configuration;
using DealnetPortal.Utilities.DataAccess;
using DealnetPortal.Utilities.Logging;
using DealnetPortal.Utilities.Messaging;
using Microsoft.AspNet.Identity;
using Unity.WebApi;

namespace DealnetPortal.Api
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();
            RegisterTypes(container);

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
        }

        public static void RegisterTypes(IUnityContainer container)
        {
            var configReader = new AppConfiguration(WebConfigSections.AdditionalSections);

            container.RegisterType<IDatabaseFactory, DatabaseFactory>(new PerResolveLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerResolveLifetimeManager());

            #region Repositories
            container.RegisterType<IContractRepository, ContractRepository>();
            container.RegisterType<IFileRepository, FileRepository>();
            container.RegisterType<IApplicationRepository, ApplicationRepository>();
            container.RegisterType<ISettingsRepository, SettingsRepository>();
            container.RegisterType<ICustomerFormRepository, CustomerFormRepository>();
            container.RegisterType<IDealerRepository, DealerRepository>();
            container.RegisterType<IRateCardsRepository, RateCardsRepository>();
            #endregion
            #region Services
            container.RegisterType<ILoggingService, LoggingService>();
            container.RegisterType<IContractService, ContractService>();
            container.RegisterType<ICustomerFormService, CustomerFormService>();
            container.RegisterType<ISignatureService, SignatureService>();
            container.RegisterType<IMailService, MailService>();
            container.RegisterType<IEmailService, EmailService>();
            container.RegisterType<IRateCardsService, RateCardsService>();
            container.RegisterType<IPersonalizedMessageService, PersonalizedMessageService>();
            container.RegisterType<IMailСhimpService, MailChimpService>();
            container.RegisterType<ISmsSubscriptionService, SmsSubscriptionService>();
            container.RegisterType<IMandrillService, MandrillService>();
            #endregion

            container.RegisterType<IHttpApiClient, HttpApiClient>("AspireClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(configReader.GetSetting(WebConfigKeys.ASPIRE_APIURL_CONFIG_KEY)));
            container.RegisterType<IHttpApiClient, HttpApiClient>("EcoreClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(configReader.GetSetting("EcoreApiUrl")));
            container.RegisterType<IHttpApiClient, HttpApiClient>("CustomerWalletClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(configReader.GetSetting(WebConfigKeys.CW_APIURL_CONFIG_KEY)));

            container.RegisterType<IAspireServiceAgent, AspireServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("AspireClient")));
            container.RegisterType<IAspireService, AspireService>();

            container.RegisterType<ICustomerWalletServiceAgent, CustomerWalletServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("CustomerWalletClient")));
            container.RegisterType<ICustomerWalletService, CustomerWalletService>();

            var queryFolderName = configReader.GetSetting(WebConfigKeys.QURIES_FOLDER_CONFIG_KEY);
            var queryFolder = HostingEnvironment.MapPath($"~/{queryFolderName}") ?? queryFolderName;

            container.RegisterType<IQueriesStorage, QueriesFileStorage>(new InjectionConstructor(queryFolder));
            container.RegisterType<IDatabaseService, MsSqlDatabaseService>(
                new InjectionConstructor(ConfigurationManager.ConnectionStrings["AspireConnection"].ConnectionString));
            container.RegisterType<IAspireStorageReader, AspireStorageReader>();
            container.RegisterType<IUsersService, UsersService>();

            container.RegisterType<IESignatureServiceAgent, ESignatureServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("EcoreClient"), new ResolvedParameter<ILoggingService>()));
            container.RegisterType<ISignatureEngine, DocuSignSignatureEngine>();
            container.RegisterType<IPdfEngine, PdfSharpEngine>();
            //container.RegisterType<ISignatureEngine, EcoreSignatureEngine>();
            container.RegisterType<ISignatureService, SignatureService>();
            container.RegisterType<IMailService, MailService>();
            container.RegisterType<IEmailService, EmailService>();
            container.RegisterType<IDealerService, DealerService>();
            container.RegisterType<IBackgroundSchedulerService, BackgroundSchedulerService>();
            container.RegisterType<IAppConfiguration, AppConfiguration>(new ContainerControlledLifetimeManager(), new InjectionConstructor(WebConfigSections.AdditionalSections as object));
        }
    }
}