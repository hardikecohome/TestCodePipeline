using DealnetPortal.Api.Common.Enumeration;


namespace DealnetPortal.Web.Models
{
    public class RateCardViewModel
    {
        public int Id { get; set; }

        public RateCardType CardType { get; set; }

        public double LoanValueFrom { get; set; }

        public double LoanValueTo { get; set; }

        public double CustomerRate { get; set; }

        public double DealerCost { get; set; }

        public double AdminFee { get; set; }

        public decimal LoanTerm { get; set; }

        public decimal AmortizationTerm { get; set; }

        public decimal DeferralPeriod { get; set; }
        public CustomerRiskGroupViewModel CustomerRiskGroup { get; set; }

        public bool IsPromo { get; set; }
    }
}