using System;
using System.Web.Mvc;
using System.Web.Routing;
using DealnetPortal.Web.Common.Culture;

namespace DealnetPortal.Web.Infrastructure
{
    public class LocalizedControllerActivator : IControllerActivator
    {
        private readonly ICultureManager _cultureManager;

        public LocalizedControllerActivator(ICultureManager cultureManager)
        {
            _cultureManager = cultureManager;
        }

        public IController Create(RequestContext requestContext, Type controllerType)
        {
            _cultureManager.EnsureCorrectCulture(requestContext.RouteData.Values["culture"] as string);
            return DependencyResolver.Current.GetService(controllerType) as IController;
        }
    }
}