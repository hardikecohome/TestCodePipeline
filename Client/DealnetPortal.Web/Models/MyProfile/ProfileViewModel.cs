using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class ProfileViewModel
    {
        public int ProfileId { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public IList<EquipmentTypeDTO> DealerEquipments { get; set; }
        public IList<DealerAreaViewModel> PostalCodes { get; set; }
        public bool QuebecDealer { get; set; }
        public string QuebecPostalCodes { get; set; }

    }
}