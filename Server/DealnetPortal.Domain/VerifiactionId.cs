using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class VerifiactionId
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string VerificationIdName { get; set; }
        public string VerificationIdNameResource { get; set; }
    }
}
