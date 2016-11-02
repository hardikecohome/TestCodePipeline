namespace DealnetPortal.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NewEquipment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public decimal? Cost { get; set; }
        public decimal? MonthlyCost { get; set; }
        public DateTime? EstimatedInstallationDate { get; set; }
        public int EquipmentInfoId { get; set; }
        [ForeignKey("EquipmentInfoId")]
        public EquipmentInfo EquipmentInfo { get; set; }
    }
}
