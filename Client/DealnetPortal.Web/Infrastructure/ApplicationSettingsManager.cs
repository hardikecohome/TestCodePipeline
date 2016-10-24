using System;
using System.Diagnostics;
using DealnetPortal.Web.Common;

namespace DealnetPortal.Web.Infrastructure
{
    public static class ApplicationSettingsManager
    {
        public static PortalType PortalType { get; private set; }
        public static bool IsOdiPortal => PortalType == PortalType.Odi;
        public static bool IsEcohomePortal => PortalType == PortalType.Ecohome;

        public static void Initialize()
        {
            //Deliberate call of Enum.Parse instead of Enum.TryParse to avoid incorrect value in config
            PortalType = (PortalType)Enum.Parse(typeof(PortalType), System.Configuration.ConfigurationManager.AppSettings["PortalType"], true);
        }
    }
}
