using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class ContactInfo
    {
        public ContactInfo()
        {
            Phones = new HashSet<Phone>();
        }

        [ForeignKey("Contract")]
        public int ContactInfoId { get; set; }
        public virtual ICollection<Phone> Phones { get; set; }
        public string EmailAddress { get; set; }
        public double? HouseSize { get; set; }

        public virtual Contract Contract { get; set; }
    }
}
