using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class RateReductionCard
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LoanTerm { get; set; }

        public int AmortizationTerm { get; set; }

        public decimal CustomerReduction { get; set; }

        public decimal InterestRateReduction { get; set; }
    }
}
