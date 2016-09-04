using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class ContractAddressDTO
    {
        public int Id { get; set; }

        public AddressType AddressType { get; set; }

        [MinLength(2)]
        [MaxLength(20)]
        public string Street { get; set; }
        [MinLength(1)]
        [MaxLength(10)]
        public string Unit { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }

        public int ContractId { get; set; }
    }
}
