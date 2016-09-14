using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class LocationDTO
    {
        public int Id { get; set; }

        public AddressType AddressType { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string Street { get; set; }
        [MaxLength(10)]
        public string Unit { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public int CustomerId { get; set; }
    }
}
