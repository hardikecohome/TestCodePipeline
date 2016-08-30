using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.Api.Models
{
    public class ContractData
    {
        public int Id { get; set; }
        public IList<HomeOwner> HomeOwners { get; set; }
        public ContractAddress ContractAddress { get; set; }        
    }
}
