using System.Collections.Generic;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Api.Models.DealerOnboarding;

namespace DealnetPortal.Web.Models.Dealer
{
    public class DealerOnboardingDictionariesViewModel
    {
        public IList<ProvinceTaxRateDTO> ProvinceTaxRates { get; set; }

        public IList<EquipmentTypeDTO> EquipmentTypes { get; set; }

        public IList<LicenseDocumentDTO> LicenseDocuments { get; set; }
    }
}