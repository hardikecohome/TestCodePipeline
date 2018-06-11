namespace DealnetPortal.Web.Models
{
    public class SubmittedCustomerFormViewModel
    {
        public decimal CreditAmount { get; set; }
        public string DealerName { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public int? CustomerBeacon { get; set; }
        public bool IsPreApproved { get; set; }
        public bool IsLeaseTypeDealer { get; set; }
    }
}
