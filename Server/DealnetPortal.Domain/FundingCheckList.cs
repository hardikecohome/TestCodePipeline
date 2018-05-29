using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class FundingCheckList
    {
        public FundingCheckList()
        {
            FundingCheckDocuments = new HashSet<FundingCheckDocument>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int ListId { get; set; }

        [Required]
        [MaxLength(2)]
        public string Province { get; set; }

        public ICollection<FundingCheckDocument> FundingCheckDocuments { get; set; }

    }
}
