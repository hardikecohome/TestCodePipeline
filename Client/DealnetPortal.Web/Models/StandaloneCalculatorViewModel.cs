using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models
{
    public class StandaloneCalculatorViewModel
    {
        public Dictionary<string, string> Plans { get; set; }
        public Dictionary<string, string> DeferralPeriods { get; set; }
        public EquipmentInformationViewModel Equipment { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        
        [CustomRequired]
        public string ProvinceTaxRate { get; set; }
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public TierDTO DealerTier { get; set; }
    }
}