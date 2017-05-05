using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models.MyProfile
{
    public class ProfileViewModel
    {

        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public IList<string> Categories { get; set; }
    }
}