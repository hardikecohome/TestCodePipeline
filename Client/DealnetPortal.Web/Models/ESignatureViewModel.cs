using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class ESignatureViewModel
    {
        public int ContractId { get; set; }
        public int HomeOwnerId { get; set; }

        public SigneeViewModel Borrower { get; set; }

        public IList<SigneeViewModel> AdditionalApplicants { get; set; }

        public SigneeViewModel SalesRep { get; set; }
    }
}