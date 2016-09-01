using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class ContractDTO
    {
        public ContractDTO()
        {
            Customers = new List<CustomerDTO>();
        }
        public int Id { get; set; }

        public ContractState ContractState { get; set; }

        public ContractAddressDTO ContractAddress { get; set; }

        public DateTime CreationTime { get; set; }

        public DateTime? LastUpdateTime { get; set; }

        public List<CustomerDTO> Customers { get; set; }
    }
}
