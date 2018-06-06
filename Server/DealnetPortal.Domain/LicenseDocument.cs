﻿using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class LicenseDocument
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int ProvinceId { get; set; }
        [ForeignKey("ProvinceId")]
        public virtual ProvinceTaxRate Province { get; set; }

        public int EquipmentTypeId { get; set; }
        [ForeignKey("EquipmentTypeId")]
        public virtual EquipmentType Equipment { get; set; }

        public int LicenseId { get; set; }
        [ForeignKey("LicenseId")]
        public virtual LicenseType License { get; set; }

    }
}
