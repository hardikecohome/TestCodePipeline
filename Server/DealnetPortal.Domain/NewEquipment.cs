namespace DealnetPortal.Domain
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    public class NewEquipment
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Quantity { get; set; }

        public string Description { get; set; }


        public double Cost { get; set; }

        public double MonthlyCost { get; set; }

        public DateTime EstimatedInstallationDate { get; set; }
        
        public double TotalMonthlyPayment { get; set; }

        public int? EquipmentInfoId { get; set; }

        [ForeignKey("EquipmentInfoId")]
        public EquipmentInfo EquipmentInfo { get; set; }
    }
}
