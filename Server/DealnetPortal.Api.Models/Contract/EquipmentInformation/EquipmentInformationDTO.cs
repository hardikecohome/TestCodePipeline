namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EquipmentInformationDTO
    {
        public int Id { get; set; }
        public List<NewEquipmentInformationDTO> NewEquipment { get; set; }
        public List<ExistingEquipmentInformationDTO> ExistingEquipment { get; set; }
        
        public string RequestedTerm { get; set; }
        
        public string SalesRep { get; set; }
        
        public string Notes { get; set; }
    }
}
