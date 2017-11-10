using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class ESignatureViewModel
    {
        public int ContractId { get; set; }
        public int HomeOwnerId { get; set; }

        public IList<SigneeViewModel> Signers { get; set; }
    }
}