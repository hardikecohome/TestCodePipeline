using System.ComponentModel.DataAnnotations;

namespace DealnetPortal.Domain
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;

    public class EquipmentInfo
    {

        public EquipmentInfo()
        {
            this.NewEquipment = new List<NewEquipment>();
            this.ExistingEquipment = new List<ExistingEquipment>();
        }

        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [ForeignKey("Contract")]
        public int Id { get; set; }
        public ICollection<NewEquipment> NewEquipment { get; set; }
        public ICollection<ExistingEquipment> ExistingEquipment { get; set; }
        
        public string RequestedTerm { get; set; }
        
        public string SalesRep { get; set; }
        
        public string Notes { get; set; }
        
        public Contract Contract { get; set; }
    }
}
