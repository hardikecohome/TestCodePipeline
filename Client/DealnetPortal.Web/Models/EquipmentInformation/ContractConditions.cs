using DealnetPortal.Web.Infrastructure.Attributes;

namespace DealnetPortal.Web.Models.EquipmentInformation
{
    public class ContractConditions
    {
        [CustomRequired]
        public bool? IsAdminFeePaidByCustomer { get; set; }

        public bool? RateCardValid { get; set; }

        public string DealProvince { get; set; }

        public bool IsClarityDealer { get; set; } = false;

        public bool IsOldClarityDeal { get; set; } = false;

        public bool? IsClarityProgram { get; set; }

        public bool IsCustomRateCardSelected { get; set; }

        public bool IsBeaconUpdated { get; set; }

        public bool IsCustomerFoundInCreditBureau { get; set; }

        public bool IsSubmittedWithoutCustomerRateCard { get; set; }

        public bool IsAllInfoCompleted { get; set; }

        public bool IsApplicantsInfoEditAvailable { get; set; }       

        public bool IsNewContract { get; set; }
        public bool FullUpdate { get; set; }
        public bool? HasExistingAgreements { get; set; }

        public bool IsDefaultRentalTier { get; set; }
        public decimal? RentalEscalatedMonthlyLimit { get; set; }
        public decimal? RentalNonEscalatedMonthlyLimit { get; set; }
        public decimal? LoanCreditAmount { get; set; }
    }
}