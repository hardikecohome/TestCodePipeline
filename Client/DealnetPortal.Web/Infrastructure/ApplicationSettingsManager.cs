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

        public static string PortalId
        {
            get
            {
                switch (PortalType)
                {
                    case PortalType.Ecohome:
                    default:
                        return "df460bb2-f880-42c9-aae5-9e3c76cdcd0f";
                    case PortalType.Odi:
                        return "606cfa8b-0e2c-47ef-b646-66c5f639aebd";
                }
            }
        }

        public static void Initialize()
        {
            //Deliberate call of Enum.Parse instead of Enum.TryParse to avoid incorrect value in config
            PortalType = (PortalType)Enum.Parse(typeof(PortalType), System.Configuration.ConfigurationManager.AppSettings["PortalType"], true);
        }
    }
}
