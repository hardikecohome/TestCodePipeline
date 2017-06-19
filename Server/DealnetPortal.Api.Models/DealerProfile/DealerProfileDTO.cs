using System;
using System.Collections.Generic;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Contract.EquipmentInformation;

namespace DealnetPortal.Api.Models.Profile
{
    public class DealerProfileDTO
    {
        public int Id { get; set; }               

        public string DealerId { get; set; }

        public IList<DealerEquipmentDTO> EquipmentList { get; set; }

        public IList<DealerAreaDTO> PostalCodesList { get; set; }
    

    }
}
