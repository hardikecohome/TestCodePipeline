using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class AdministrativeAreasHelper
    {
        public static string ToProvinceAbbreviation(this string name)
        {
            if (name == null)
            {
                return null;
            }
            switch (name.ToUpper())
            {
                case "AB":
                case "ALBERTA":
                    return "AB";
                case "BC":
                case "BRITISH COLUMBIA":
                    return "BC";
                case "MB":
                case "MANITOBA":
                    return "MB";
                case "NB":
                case "NEW BRUNSWICK":
                    return "NB";
                case "NL":
                case "NEWFOUNDLAND AND LABRADOR":
                    return "NL";
                case "NT":
                case "NORTHWEST TERRITORIES":
                    return "NT";
                case "NS":
                case "NOVA SCOTIA":
                    return "NS";
                case "NU":
                case "NUNAVUT":
                    return "NU";
                case "ON":
                case "ONTARIO":
                    return "ON";
                case "PE":
                case "PRINCE EDWARD ISLAND":
                    return "PE";
                case "QC":
                case "QUEBEC":
                    return "QC";
                case "SK":
                case "SASKATCHEWAN":
                    return "SK";
                case "YT":
                case "YUKON":
                    return "YT";
                default:
                    return null;
            }
        }
    }
}
