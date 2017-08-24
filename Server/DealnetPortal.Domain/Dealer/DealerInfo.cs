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
        public DealerInfo()
        {
            Owners = new HashSet<OwnerInfo>();
            RequiredDocuments = new HashSet<RequiredDocument>();
        }

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
        [Index("AccessCode", IsUnique = true)]
        public string AccessCode { get; set; }

        public DateTime CreationTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }

        public int? CompanyInfoId { get; set; }
        [ForeignKey(nameof(CompanyInfoId))]
        public virtual CompanyInfo CompanyInfo { get; set; }

        public virtual OwnerInfo PrimaryOwner { get; set; }
        public virtual ICollection<OwnerInfo> Owners { get; set; }

        public int? ProductInfoId { get; set; }
        [ForeignKey(nameof(ProductInfoId))]
        public virtual ProductInfo ProductInfo { get; set; }
        
        public ICollection<RequiredDocument> RequiredDocuments { get; set; } 
    }
}
