using System;
using DealnetPortal.Api.Common.ApiClient;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Core.Culture;
using DealnetPortal.Web.Core.Security;
using DealnetPortal.Web.Core.Services;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;

namespace DealnetPortal.Web.App_Start
{
    /// <summary>
    /// Specifies the Unity configuration for the main container.
    /// </summary>
    public class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        /// <summary>Registers the type mappings with the Unity container.</summary>
        /// <param name="container">The unity container to configure.</param>
        /// <remarks>There is no need to register concrete types such as controllers or API controllers (unless you want to 
        /// change the defaults), as Unity allows resolving a concrete type even if it was not previously registered.</remarks>
        public static void RegisterTypes(IUnityContainer container)
        {
            // NOTE: To load from web.config uncomment the line below. Make sure to add a Microsoft.Practices.Unity.Configuration to the using statements.
            // container.LoadConfiguration();

            // TODO: Register your types here
            // container.RegisterType<IProductRepository, ProductRepository>();
            container.RegisterType<IHttpApiClient, HttpApiClient>(new ContainerControlledLifetimeManager(), new InjectionConstructor(System.Configuration.ConfigurationManager.AppSettings["ApiUrl"]));
            container.RegisterType<ISecurityServiceAgent, SecurityServiceAgent>();
            container.RegisterType<IUserManagementServiceAgent, UserManagementServiceAgent>();
            container.RegisterType<IContractServiceAgent, ContractServiceAgent>();
            container.RegisterType<ICultureManager, CultureManager>();
            container.RegisterType<ISecurityManager, SecurityManager>(new InjectionConstructor(typeof(ISecurityServiceAgent), typeof(IUserManagementServiceAgent), typeof(ILoggingService), ApplicationSettingsManager.PortalType));
            container.RegisterType<ILoggingService, LoggingService>();
            container.RegisterType<IScanProcessingServiceAgent, ScanProcessingServiceAgent>();
            container.RegisterType<IContractServiceAgent, ContractServiceAgent>();
            container.RegisterType<IDictionaryServiceAgent, DictionaryServiceAgent>();
            container.RegisterType<IContractManager, ContractManager>();
            container.RegisterType<ICacheService, MemoryCacheService>();
            container.RegisterType<ISettingsManager, SettingsManager>();
        }
    }
}
