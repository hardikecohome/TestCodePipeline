using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    using EquipmentInformation;

    public class ContractDTO
    {
        public ContractDTO()
        {
            SecondaryCustomers = new List<CustomerDTO>();
        }
        public int Id { get; set; }

        public ContractState ContractState { get; set; }       

        public DateTime CreationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public CustomerDTO PrimaryCustomer { get; set; }

        public List<CustomerDTO> SecondaryCustomers { get; set; }

        public ContractDetailsDTO Details { get; set; }

        public PaymentInfoDTO PaymentInfo { get; set; }

        public EquipmentInfoDTO Equipment { get; set; }

        public List<CommentDTO> Comments { get; set; }
        
        public List<ContractDocumentDTO> Documents { get; set; }
    }
}
