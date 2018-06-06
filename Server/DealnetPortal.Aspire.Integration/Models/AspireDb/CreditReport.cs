using System;

namespace DealnetPortal.Aspire.Integration.Models.AspireDb
{
    public class CreditReport
    {
        public string CustomerId { get; set; }
        public string ContractId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public DateTime? CreatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public string LastUpdateUser { get; set; }

        public string CreditId { get; set; }
        public int Beacon { get; set; }
    }
}
