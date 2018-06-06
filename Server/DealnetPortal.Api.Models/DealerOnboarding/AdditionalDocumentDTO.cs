using System;

namespace DealnetPortal.Api.Models.DealerOnboarding
{
    public class AdditionalDocumentDTO
    {
        public int Id { get; set; }

        public string Number { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public int DealerInfoId { get; set; }

        public bool NotExpired { get; set; }

        public LicenseTypeDTO License { get; set; }
    }
}
