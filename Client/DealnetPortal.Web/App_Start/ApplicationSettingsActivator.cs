using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Web.Infrastructure;

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
