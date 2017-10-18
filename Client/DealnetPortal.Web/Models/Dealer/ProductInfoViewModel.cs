using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration.Dealer;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Models.Dealer
{
    public class ProductInfoViewModel
    {
        public ProductInfoViewModel()
        {
            Brands = new List<string> {};
            EquipmentTypes = new List<EquipmentTypeDTO>();
        }

        [CustomRequired]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string PrimaryBrand { get; set; }
        
        public List<string> Brands { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalesVolume")]
        [RegularExpression(@"^[0-9]\d{0,10}?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AnnualSalesIncorrectFormat")]
        public string AnnualSalesVolume { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AverageTransactionSize")]
        [RegularExpression(@"^[0-9]\d{0,10}?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "AverageTransactionSizeIncorrectFormat")]
        public string AverageTransactionSize { get; set; }

        public bool SalesApproachConsumerDirect { get; set; }
        public bool SalesApproachBroker { get; set; }
        public bool SalesApproachDistributor { get; set; }
        public bool SalesApproachDoorToDoor { get; set; }

        public bool LeadGenReferrals { get; set; }
        public bool LeadGenLocalAds { get; set; }
        public bool LeadGenTradeShows { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "PreferredFinancingProductsRequired")]
        public ProgramServices? ProgramService { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "RelationshipTo")]
        public RelationshipStructure? Relationship { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "OEMName")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string OEMName { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "AreYouWithProvider")]
        public bool WithCurrentProvider { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "FinanceProviderName")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "TheFieldMustBeMinimumAndMaximum")]
        public string FinanceProviderName { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "MonthlyFinancedValue")]
        [RegularExpression(@"^[0-9]\d{0,11}?$", ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "MonthlyFinancedValueIncorrectFormat")]
        public string MonthlyFinancedValue { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "OfferMonthlyDeferrals")]
        public bool OfferMonthlyDeferrals { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PercentMonthlyDealsDeferred")]
        [Range(0,100, ErrorMessageResourceType =typeof(Resources.Resources), ErrorMessageResourceName = "MonthlyDefferedPercentRange")]
        [RegularExpression(@"^[0-9](\d{0,2}?)$", ErrorMessageResourceType =typeof(Resources.Resources), ErrorMessageResourceName = "PercentMonthDeferredIncorrectFormat")]
        public double? PercentMonthlyDealsDeferred { get; set; }

        [CustomRequired]
        public List<EquipmentTypeDTO> EquipmentTypes { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ReasonForInterestWithEcohome")]
        public ReasonForInterest? ReasonForInterest { get; set; }
    }
}