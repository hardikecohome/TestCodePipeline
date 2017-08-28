﻿using System.Collections.Generic;
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
            Brands = new List<string> {""};
            EquipmentTypes = new List<EquipmentTypeDTO>();
            SalesApproach = new SalesApproachViewModel();
            LeadGenMethods = new LeadsGenerationViewModel();
        }

        [MinMaxListCount(1, 3, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "BrandsLength")]
        public List<string> Brands { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AnnualSalesVolume")]
        public decimal AnnualSalesVolume { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "AverageTransactionSize")]
        public decimal AverageTransactionSize { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "SalesApproach")]
        public SalesApproachViewModel SalesApproach { get; set; }
        
        [Display(ResourceType = typeof(Resources.Resources), Name = "LeadGenMethods")]
        public LeadsGenerationViewModel LeadGenMethods { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "ProgramServiceRequired")]
        public ProgramServices? ProgramService { get; set; }

        [CustomRequired]
        [Display(ResourceType = typeof(Resources.Resources), Name = "RelationshipStructure")]
        public RelationshipStructure Relationship { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "OEMName")]
        public string OEMName { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "AreYouWithProvider")]
        public bool WithCurrentProvider { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "FinanceProviderName")]
        public string FinanceProviderName { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "MonthlyFinancedValue")]
        public decimal? MonthlyFinancedValue { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "OfferMonthlyDeferrals")]
        public bool OfferMonthlyDeferrals { get; set; }

        [Display(ResourceType = typeof(Resources.Resources), Name = "PercentMonthlyDealsDeferred")]
        public int PercentMonthlyDealsDeferred { get; set; }

        [CustomRequired]
        [MinMaxListCount(1, int.MaxValue, ErrorMessageResourceType = typeof(Resources.Resources), ErrorMessageResourceName = "EquipmentTypeLength")]
        public List<EquipmentTypeDTO> EquipmentTypes { get; set; }

        [CustomRequired]
        public ReasonForInterest ReasonForInterest { get; set; }
    }
}