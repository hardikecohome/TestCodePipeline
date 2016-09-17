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
            Locations = new List<Location>();
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
        //[Required]
        // 1 customer for 1 contract ? 
        //public Contract Contract { get; set; }
        // Is locations related to a customer or to a contract
        public ICollection<Location> Locations { get; set; }
    }
}
