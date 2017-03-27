using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    /// <summary>
    /// Options set for selected dealer and language
    /// </summary>
    public class CustomerLinkLanguageOptionsDTO
    {
        /// <summary>
        /// Is requested language available?
        /// </summary>
        public bool IsLanguageEnabled { get; set; }
        /// <summary>
        /// Codes of enabled languages
        /// </summary>
        public List<LanguageCode> EnabledLanguages { get; set; }
        /// <summary>
        /// Services for selected language
        /// </summary>
        public List<string> LanguageServices { get; set; }
    }
}
