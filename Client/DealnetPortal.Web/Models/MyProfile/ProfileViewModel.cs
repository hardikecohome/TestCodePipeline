using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.Profile;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class ProfileViewModel
    {
        public int ProfileId { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public IList<EquipmentTypeDTO> DealerEquipments { get; set; }
        public IList<DealerAreaViewModel> PostalCodes { get; set; }

    }
}