﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain.Dealer
{
    public class AdditionalDocument
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int LicenseTypeId { get; set; }
        [ForeignKey("LicenseTypeId")]
        public virtual LicenseType License { get; set; }

        public string Number { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public bool NotExpired { get; set; }

        public int DealerInfoId { get; set; }
        [ForeignKey(nameof(DealerInfoId))]
        [Required]
        public virtual DealerInfo DealerInfo { get; set; }
    }
}
