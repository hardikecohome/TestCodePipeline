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
        public string SupportType { get; set; }
        public string HelpRequested { get; set; }
        public BestWayDTO BestWay { get; set; }        
    }

    public class BestWayDTO
    {
        public bool byPhone { get; set; }
        public bool SameEmail { get; set; }
        public bool AlternativeEmail { get; set; }
        public string AlternativeEmailAddress { get; set; }
    }




}
