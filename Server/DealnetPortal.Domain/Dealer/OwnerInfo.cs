using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain.Dealer
{
    public class OwnerInfo
    {
        public OwnerInfo()
        {
            Address = new AddressBase();
        }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string EmailAddress { get; set; }

        public AddressBase Address{ get; set; }

        public decimal? PercentOwnership { get; set; }

        public int OwnerOrder { get; set; }

        public int DealerInfoId { get; set; }
        [ForeignKey("DealerInfoId")]
        [Required]
        public DealerInfo DealerInfo { get; set; }
    }
}
