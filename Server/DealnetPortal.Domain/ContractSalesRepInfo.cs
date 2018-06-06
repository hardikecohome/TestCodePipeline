using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class ContractSalesRepInfo
    {
        [ForeignKey("Contract")]
        public int Id { get; set; }
        public Contract Contract { get; set; }
        
        public bool InitiatedContact { get; set; }

        public bool NegotiatedAgreement { get; set; }

        public bool ConcludedAgreement { get; set; }
    }
}
