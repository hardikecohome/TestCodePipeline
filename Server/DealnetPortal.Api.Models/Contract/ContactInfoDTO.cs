using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class ContactInfoDTO
    {
        public int Id { get; set; }
        public IList<PhoneDTO> Phones { get; set; }
        public string EmailAddress { get; set; }
        public double? HouseSize { get; set; }
    }
}
