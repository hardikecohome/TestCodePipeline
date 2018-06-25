﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DealnetPortal.Domain
{
    public class InstallationPackage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal? MonthlyCost { get; set; }        

        public int EquipmentInfoId { get; set; }
        [ForeignKey("EquipmentInfoId")]
        [Required]
        public EquipmentInfo EquipmentInfo { get; set; }
    }
}
