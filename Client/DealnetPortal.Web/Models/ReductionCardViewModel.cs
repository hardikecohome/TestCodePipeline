namespace DealnetPortal.Web.Models
{
	public class ReductionCardViewModel
	{
		public long Id { get; set; }
		public string LoanAmortizationTerm { get; set; }

		public decimal CustomerReduction { get; set; }

		public decimal InterestRateReduction { get; set; }
	}
}