﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class FundingCheckList
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(2)]
        public string Province { get; set; }

        public string DealerId { get; set; }
        [ForeignKey("DealerId")]
        public virtual ApplicationUser Dealer { get; set; }

        public int ListId { get; set; }
        [ForeignKey(nameof(ListId))]
        public FundingDocumentList FundingDocumentList { get; set; }

    }
}
