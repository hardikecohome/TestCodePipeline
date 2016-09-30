using Microsoft.Practices.Unity;
using System.Web.Http;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Api.Controllers;
using DealnetPortal.Api.Integration.ServiceAgents;
using DealnetPortal.Api.Integration.ServiceAgents.ESignature;
using DealnetPortal.Api.Integration.Services;
using DealnetPortal.DataAccess;
using DealnetPortal.DataAccess.Repositories;
using DealnetPortal.Utilities;
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

            container.RegisterType<IContractService, ContractService>();            

            container.RegisterType<AccountController>(new InjectionConstructor());

            container.RegisterType<IHttpApiClient, HttpApiClient>("AspireClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(System.Configuration.ConfigurationManager.AppSettings["AspireApiUrl"]));
            container.RegisterType<IHttpApiClient, HttpApiClient>("EcoreClient", new ContainerControlledLifetimeManager(), new InjectionConstructor(System.Configuration.ConfigurationManager.AppSettings["EcoreApiUrl"]));

            container.RegisterType<IAspireServiceAgent, AspireServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("AspireClient")));
            container.RegisterType<IAspireService, AspireService>();

            container.RegisterType<IESignatureServiceAgent, ESignatureServiceAgent>(new InjectionConstructor(new ResolvedParameter<IHttpApiClient>("EcoreClient"), new ResolvedParameter<ILoggingService>()));
            container.RegisterType<ISignatureService, SignatureService>();
        }
    }
}