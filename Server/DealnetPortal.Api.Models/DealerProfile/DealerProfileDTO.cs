using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Models.Profile
{
    public class DealerProfileDTO
    {
        public int Id { get; set; }               

        public string DealerId { get; set; }

        public AddressDTO Address { get; set; }

        public string EmailAddress { get; set; }

        public string Phone { get; set; }

        public string Culture { get; set; }

        public IList<DealerEquipmentDTO> EquipmentList { get; set; }

        public IList<DealerAreaDTO> PostalCodesList { get; set; }
    

    }
}
