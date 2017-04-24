using System.Configuration;
using System.Web.Hosting;
using Microsoft.Practices.Unity;
using System.Web.Http;
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
            container.RegisterType<ILoggingService, LoggingService>();

            container.RegisterType<IDatabaseFactory, DatabaseFactory>(new PerResolveLifetimeManager());
            container.RegisterType<IUnitOfWork, UnitOfWork>(new PerResolveLifetimeManager());

            container.RegisterType<IContractRepository, ContractRepository>();
            container.RegisterType<IFileRepository, FileRepository>();
            container.RegisterType<IApplicationRepository, ApplicationRepository>();
            container.RegisterType<ISettingsRepository, SettingsRepository>();
            container.RegisterType<ICustomerFormRepository, CustomerFormRepository>();
            container.RegisterType<IDealerRepository, DealerRepository>();

            container.RegisterType<IContractService, ContractService>();
            container.RegisterType<ICustomerFormService, CustomerFormService>();

            container.RegisterType<IHttpApiClient, HttpApiClient>("AspireClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(System.Configuration.ConfigurationManager.AppSettings["AspireApiUrl"]));
            container.RegisterType<IHttpApiClient, HttpApiClient>("EcoreClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(System.Configuration.ConfigurationManager.AppSettings["EcoreApiUrl"]));

            container.RegisterType<IAspireServiceAgent, AspireServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("AspireClient")));
            container.RegisterType<IAspireService, AspireService>();

            var queryFolderName = ConfigurationManager.AppSettings["QueriesFolder"] ?? "Queries";
            var queryFolder = HostingEnvironment.MapPath($"~/{queryFolderName}") ?? queryFolderName;

            container.RegisterType<IQueriesStorage, QueriesFileStorage>(new InjectionConstructor(queryFolder));
            container.RegisterType<IDatabaseService, MsSqlDatabaseService>(
                new InjectionConstructor(System.Configuration.ConfigurationManager.ConnectionStrings["AspireConnection"].ConnectionString));
            container.RegisterType<IAspireStorageService, AspireStorageService>();

            container.RegisterType<IESignatureServiceAgent, ESignatureServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("EcoreClient"), new ResolvedParameter<ILoggingService>()));
            container.RegisterType<ISignatureEngine, DocuSignSignatureEngine>();
            container.RegisterType<IPdfEngine, PdfSharpEngine>();
            //container.RegisterType<ISignatureEngine, EcoreSignatureEngine>();
            container.RegisterType<ISignatureService, SignatureService>();
            container.RegisterType<IMailService, MailService>();
            container.RegisterType<IEmailService, EmailService>();

        }
    }
}