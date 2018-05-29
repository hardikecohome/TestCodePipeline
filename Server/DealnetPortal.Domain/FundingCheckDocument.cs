using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class FundingCheckDocument
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ListId { get; set; }
        [ForeignKey("ListId")]
        public virtual FundingCheckList FundingCheckList { get; set; }

        public int DocumentTypeId { get; set; }
        [ForeignKey("DocumentTypeId")]
        public DocumentType DocumentType { get; set; }
    }
}
