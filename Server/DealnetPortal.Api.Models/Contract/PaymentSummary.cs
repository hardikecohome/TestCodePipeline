using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class PaymentSummary
    {
        public decimal? MonthlyPayment { get; set; }
        public decimal? Hst { get; set; }
        public decimal? TotalPayment { get; set; }
        public decimal? TotalAllMonthlyPayment { get; set; }
    }
}
