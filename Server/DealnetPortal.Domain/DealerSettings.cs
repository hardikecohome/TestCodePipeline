using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Domain
{
    /// <summary>
    ///Dealer settings, affected to a business flow: program types, tiers, etc.
    /// </summary>
    public class DealerSettings
    {
        [ForeignKey(nameof(Dealer))]
        public string DealerId { get; set; }        
        [Required]
        public virtual ApplicationUser Dealer { get; set; }
        /// <summary>
        /// Supported program types for a dealer: NULL - both
        /// </summary>
        public AgreementType? SupportedAgreementType { get; set; }
    }
}
