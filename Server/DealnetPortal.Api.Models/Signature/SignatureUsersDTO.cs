using System.Collections.Generic;

namespace DealnetPortal.Api.Models.Signature
{
    public class SignatureUsersDTO
    {
        public int ContractId { get; set; }
        public IList<SignatureUser> Users { get; set; }
    }
}
