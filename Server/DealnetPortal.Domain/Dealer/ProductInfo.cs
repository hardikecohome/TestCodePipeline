using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration.Dealer;

namespace DealnetPortal.Domain.Dealer
{
    public class ProductInfo
    {
        public ProductInfo()
        {
            Brands = new HashSet<ManufacturerBrand>();
            Services = new HashSet<ProductService>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public decimal? AnnualSalesVolume { get; set; }
        public decimal? AverageTransactionSize { get; set; }
        public Channel? SalesApproach { get; set; }
        public ProgramServices? ProgramService { get; set; }

        public bool? LeadGenReferrals { get; set; }
        public bool? LeadGenLocalAdvertising { get; set; }
        public bool? LeadGenTradeShows { get; set; }

        public RelationshipStructure? Relationship { get; set; }
        public string OemName { get; set; }
        public bool? WithCurrentProvider { get; set; }
        public string FinanceProviderName { get; set; }
        public decimal? MonthlyFinancedValue { get; set; }
        public bool? OfferMonthlyDeferrals { get; set; }        
        public decimal? PercentMonthlyDealsDeferred { get; set; }
        public ReasonForInterest? ReasonForInterest { get; set; }

        public virtual ICollection<ManufacturerBrand> Brands { get; set; } 

        public virtual ICollection<ProductService> Services { get; set; } 
    }
}
