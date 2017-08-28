using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.Models.Dealer
{
    public class DealerOnboardingDictionariesViewModel
    {
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }

        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
    }
}