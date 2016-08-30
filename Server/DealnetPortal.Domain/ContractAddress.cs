using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain
{
    public class ContractAddress
    {        
        public int Id { get; set; }
        [MinLength(2)]
        [MaxLength(20)]
        public string Street { get; set; }
        [MinLength(1)]
        [MaxLength(10)]
        public string Unit { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        [Required]
        public Contract Contract { get; set; }
    }
}
