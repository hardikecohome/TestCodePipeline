using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class FundingDocumentList
    {
        public FundingDocumentList()
        {
            FundingCheckDocuments = new HashSet<FundingCheckDocument>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }
        public string Description { get; set; }
        public ICollection<FundingCheckDocument> FundingCheckDocuments { get; set; }
    }
}
