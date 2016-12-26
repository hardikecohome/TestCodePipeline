﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Helpers
{
    public static class CultureHelper
    {
        private static string DefaultCulture = "en";

        public static string FilterCulture(string name)
        {
            // make sure it's not null or empty
            return string.IsNullOrEmpty(name) ? GetDefaultCulture() : name;
        }

        public static string GetDefaultCulture()
        {
            return DefaultCulture;
        }
        public static string GetCurrentCulture()
        {
            return Thread.CurrentThread.CurrentCulture.Name;
        }
        public static string GetCurrentNeutralCulture()
        {
            return GetNeutralCulture(Thread.CurrentThread.CurrentCulture.Name);
        }
        public static string GetNeutralCulture(string name)
        {
            if (!name.Contains("-")) return name;
            return name.Split('-')[0]; // Read first part only. E.g. "en", "es"
        }
    }
}
