using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain.Enums;

namespace DealnetPortal.Domain
{
    public class Contract
    {
        public Contract()
        {
            Customers = new List<Customer>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]        
        public int Id { get; set; }

        public ApplicationUser Dealer { get; set; }
        public ContractState ContractState { get; set; }
        public ContractAddress ContractAddress { get; set; }

        public virtual ICollection<Customer> Customers { get; set; }
    }
}
