using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class DomainCodesHelper
    {
        public static string ToProvinceCode(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }
            switch (name.Trim().ToUpper())
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
                case "NUNAVUT TERRITORY":
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
                case "YUKON TERRITORY":
                    return "YT";
                default:
                    return name.ToUpper(); // in a case when code passed as parameter
            }
        }
    }
}
