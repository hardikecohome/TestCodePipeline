using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Aspire.Integration.Models.AspireDb
{
    public class CreditReport
    {
        public string CustomerId { get; set; }
        public string ContractId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string CrtdTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string LastUpdateUser { get; set; }

        public string CreditId { get; set; }
        public int Beacon { get; set; }
    }
}
