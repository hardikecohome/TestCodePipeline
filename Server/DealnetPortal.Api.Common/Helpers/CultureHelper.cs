using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class CultureHelper
    {
        public static string ToCultureCode(this int culture)
        {
            switch (culture)
            {
                case 0:
                default:
                    return "en";
                case 1:
                    return "fr";
            }
        }
    }
}
