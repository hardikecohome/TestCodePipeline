using System.Collections.Generic;

namespace DealnetPortal.Domain
{
    public class ContractData
    {
        public int Id { get; set; }
        public Customer PrimaryCustomer { get; set; }
        public IList<Customer> SecondaryCustomers { get; set; }
        public IList<Location> Locations { get; set; }
        public IList<Phone> Phones { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
    }
}
