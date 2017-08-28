using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    public class Location : AddressBase
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public AddressType AddressType { get; set; }
        public ResidenceType ResidenceType { get; set; }

        public DateTime? MoveInDate { get; set; }

        public Customer Customer { get; set; }

    }
}
