using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Common.Helpers
{
    public static class CultureHelper
    {
        // Valid cultures
        private static readonly List<string> ValidCultures = new List<string> { "en", "en-CA", "en-GB", "en-US", "fr", "fr-CA" };
        // Include ONLY cultures you are implementing
        private static readonly List<string> Cultures = new List<string> {
            "en",  // first culture is the DEFAULT
            "fr", // franch NEUTRAL culture        
            };

        public static string GetImplementedCulture(string name)
        {
            // make sure it's not null
            if (string.IsNullOrEmpty(name))
                return GetDefaultCulture(); // return Default culture
            // make sure it is a valid culture first
            if (!ValidCultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                return GetDefaultCulture(); // return Default culture if it is invalid
            // if it is implemented, accept it
            if (Cultures.Any(c => c.Equals(name, StringComparison.InvariantCultureIgnoreCase)))
                return name; // accept it
            // Find a close match. For example, if you have "en-US" defined and the user requests "en-GB", 
            // the function will return closes match that is "en-US" because at least the language is the same (ie English)  
            var neutralCulture = GetNeutralCulture(name);
            return Cultures.FirstOrDefault(c => c.StartsWith(neutralCulture)) ?? GetDefaultCulture();
        }

        public static string GetDefaultCulture()
        {
            return Cultures[0]; // return Default culture
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
