using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class StringHelper
    {
        public static string ConcatWithComma(this IEnumerable<string> values)
        {
            if (values == null) { return null; }
            var stb = new StringBuilder();
            var i = 0;
            foreach (var str in values)
            {
                if (i != 0) { stb.Append(", "); }
                stb.Append(str);
                i++;
            }
            return stb.ToString();
        }
    }
}
