using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class Customer
    {
        public Customer()
        {
            Locations = new HashSet<Location>();
            Phones = new HashSet<Phone>();
            Emails = new HashSet<Email>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }
        [Required]
        public DateTime DateOfBirth { get; set; }
        
        public virtual ICollection<Location> Locations { get; set; }

        public virtual ICollection<Phone> Phones { get; set; }

        public virtual ICollection<Email> Emails { get; set; }

        public string AccountId { get; set; }
    }
}
