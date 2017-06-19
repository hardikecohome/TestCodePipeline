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
        public EquipmentInformationViewModel Equipment { get; set; }

        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }

        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
    }
}