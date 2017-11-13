using DealnetPortal.Api.Common.Enumeration;
using System.Collections.Generic;

namespace DealnetPortal.Web.Models
{
    public class ESignatureViewModel
    {
        public int ContractId { get; set; }
        public int HomeOwnerId { get; set; }
        public SignatureStatus? Status { get; set; }

        public IList<SigneeViewModel> Signers { get; set; }
    }
}