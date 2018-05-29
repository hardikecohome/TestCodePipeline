using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class DealerArea
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProfileId { get; set; }
        [ForeignKey("ProfileId")]
        public virtual DealerProfile DealerProfile { get; set; }

        [MaxLength(10)]
        public string PostalCode { get; set; }
    }
}
