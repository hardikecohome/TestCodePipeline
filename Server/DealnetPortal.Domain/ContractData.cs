using System.Collections.Generic;

namespace DealnetPortal.Domain
{
    public class ContractData
    {
        public int Id { get; set; }
        public IList<ContractCustomer> Customers { get; set; }
        public IList<Location> Locations { get; set; }        
    }
}
