using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{
    public class LoanCalculatorViewModel
    {
        public List<NewEquipmentInformation> NewEquipment { get; set; }

        public List<EquipmentTypeDTO> EquipmentTypes { get; set; }


    }
}