using System.Collections.Generic;
using System.Web.Mvc;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;

namespace DealnetPortal.Web.Models
{
    public class StandaloneCalculatorViewModel
    {
        public List<string> Plans { get; set; }
        public List<string> DeferralPeriods { get; set; }
        public EquipmentInformationViewModel Equipment { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public TierDTO DealerTier { get; set; }
    }
}