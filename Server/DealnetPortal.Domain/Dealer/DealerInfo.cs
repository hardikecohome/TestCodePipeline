using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Domain.Dealer
{
    public class DealerInfo
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        /// <summary>
        /// Link to a parent dealer
        /// </summary>
        public string ParentSalesRepId { get; set; }
        [ForeignKey(nameof(ParentSalesRepId))]
        public ApplicationUser ParentSalesRep { get; set; }
        /// <summary>
        /// Link to a draft for continue editing
        /// </summary>
        [Index("LinkIndex", IsUnique = true)]
        public string DraftLink { get; set; }
    }
}
