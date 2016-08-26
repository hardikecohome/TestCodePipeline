using Microsoft.Practices.Unity;
using System.Web.Http;
using DealnetPortal.Api.Controllers;
using DealnetPortal.DataAccess;
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

            container.RegisterType<AccountController>(new InjectionConstructor());
        }
    }
}