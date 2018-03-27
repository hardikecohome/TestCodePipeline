using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models.EquipmentInformation;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models
{
    public class StandaloneCalculatorViewModel
    {
        public Dictionary<string, string> Plans { get; set; }
        public Dictionary<string, string> DeferralPeriods { get; set; }
        public EquipmentInformationViewModel Equipment { get; set; }
        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }
        public int TotalAmountFinancedFor180AmortTerm { get; set; }
        public int AdminFee { get; set; }
        [CustomRequired]
        public string ProvinceTaxRate { get; set; }
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }
        public TierDTO DealerTier { get; set; }
		public bool RateCardProgramsAvailable { get; set; }
    }
}