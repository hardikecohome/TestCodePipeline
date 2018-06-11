using DealnetPortal.Web.Infrastructure.Managers;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(DealnetPortal.Web.App_Start.ApplicationSettingsActivator), "Start", Order = 1)]

namespace DealnetPortal.Web.App_Start
{
    public class ApplicationSettingsActivator
    {
        public static void Start()
        {
            ApplicationSettingsManager.Initialize();
        }
    }
}
