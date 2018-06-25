using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class Phone
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public PhoneType PhoneType { get; set; }

        [MaxLength(50)]
        public string PhoneNum { get; set; }

        public Customer Customer { get; set; }
    }
}
