using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerLinkDTO
    {
        /// <summary>
        /// Codes of enabled languages
        /// </summary>
        public List<int> EnabledLanguages { get; set; }
        public Dictionary<int, List<string>> Services { get; set; }
    }
}
