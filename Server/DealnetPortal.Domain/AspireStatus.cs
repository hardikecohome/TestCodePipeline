using System.ComponentModel.DataAnnotations.Schema;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class AspireStatus
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Status { get; set; }
        public ContractState? ContractState { get; set; }
    }
}
