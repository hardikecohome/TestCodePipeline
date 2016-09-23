using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract.EquipmentInformation
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class EquipmentInfoDTO
    {
        public int Id { get; set; }
        public AgreementType AgreementType { get; set; }
        public List<NewEquipmentDTO> NewEquipment { get; set; }
        public List<ExistingEquipmentDTO> ExistingEquipment { get; set; }
        public decimal TotalMonthlyPayment { get; set; }

        public string RequestedTerm { get; set; }
        
        public string SalesRep { get; set; }
        
        public string Notes { get; set; }

        public int ContractId { get; set; }
    }
}
