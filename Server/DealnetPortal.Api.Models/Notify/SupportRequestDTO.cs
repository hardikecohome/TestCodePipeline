using DealnetPortal.Api.Common.Enumeration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Notify
{
    public class SupportRequestDTO
    {
        public int Id { get; set; }
        public string DealerName { get; set; }
        public string YourName { get; set; }
        public string LoanNumber { get; set; }
        public SupportType SupportType { get; set; }
        public string HelpRequested { get; set; }
        public PreferredContactType BestWay { get; set; }
        public string ContactDetails { get; set; }
    }
}
