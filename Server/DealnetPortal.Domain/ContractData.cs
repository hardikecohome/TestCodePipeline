using System.Collections.Generic;

namespace DealnetPortal.Domain
{
    public class ContractData
    {
        public int Id { get; set; }
        public IList<Customer> Customers { get; set; }
        public IList<Location> Addresses { get; set; }        
    }
}
