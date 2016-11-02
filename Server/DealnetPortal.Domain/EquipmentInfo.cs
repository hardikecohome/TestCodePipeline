using System.ComponentModel.DataAnnotations;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class EquipmentInfo
    {
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("Contract")]
        public int Id { get; set; }
        public AgreementType AgreementType { get; set; }
        public ICollection<NewEquipment> NewEquipment { get; set; }
        public ICollection<ExistingEquipment> ExistingEquipment { get; set; }
        public decimal? TotalMonthlyPayment { get; set; }

        public int RequestedTerm { get; set; } 

        public int? AmortizationTerm { get; set; }
        
        public double? CustomerRate { get; set; }
        
        public double? AdminFee { get; set; }
        
        public double? DownPayment { get; set; }

        public string SalesRep { get; set; }
        
        public string Notes { get; set; }
        
        public Contract Contract { get; set; }
    }
}
