using DealnetPortal.Api.Common.Helpers;

namespace DealnetPortal.Api.Common.Types
{
    public class PaymentSummary
    {
        public decimal? MonthlyPayment { get; set; }
        public decimal? Hst { get; set; }
        public decimal? TotalPayment { get; set; }
        public decimal? TotalAllMonthlyPayment { get; set; }
        public LoanCalculator.Output LoanDetails { get; set; }
    }
}
