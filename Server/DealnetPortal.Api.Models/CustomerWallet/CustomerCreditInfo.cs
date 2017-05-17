using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.CustomerWallet
{
    public class CustomerCreditInfo
    {
        public string AspireAccountId { get; set; }
        public string AspireTransactionId { get; set; }
        public string AspireStatus { get; set; }
        public int? ScorecardPoints { get; set; }
        public decimal? CreditAmount { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}
