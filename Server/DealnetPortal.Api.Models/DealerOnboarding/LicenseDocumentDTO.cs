using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Models.DealerOnboarding
{
    public class LicenseDocumentDTO
    {
        public int Id { get; set; }

        public virtual ProvinceTaxRateDTO Province { get; set; }

        public virtual EquipmentTypeDTO Equipment { get; set; }

        public virtual LicenseTypeDTO License { get; set; }
    }
}
