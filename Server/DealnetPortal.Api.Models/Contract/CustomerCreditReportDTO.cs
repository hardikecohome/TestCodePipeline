using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerCreditReportDTO
    {
        public int Beacon { get; set; }
        public DateTime CreditLastUpdateTime { get; set; }
    }
}
