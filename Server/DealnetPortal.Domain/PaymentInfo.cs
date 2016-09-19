using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class PaymentInfo
    {
        [ForeignKey("Contract")]
        public int Id { get; set; }
        public PaymentType PaymentType { get; set; }
        public WithdrawalDateType PrefferedWithdrawalDate { get; set; }
        public string BlankNumber { get; set; }
        public string TransitNumber { get; set; }
        public string AccountNumber { get; set; }
        public string EnbridgeGasDistributionAccount { get; set; }

        public virtual Contract Contract { get; set; }
    }
}
