using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class CreditCheckDTO
    {
        public int ContractId { get; set; }
        public CreditCheckState CreditCheckState { get; set; }

        public int Beacon { get; set; }

        public decimal CreditAmount { get; set; }

        public int ScorecardPoints { get; set; }
    }
}
