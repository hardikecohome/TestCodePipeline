using System;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Models.DealerOnboarding
{
    public class OwnerInfoDTO
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }

        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public string EmailAddress { get; set; }

        public AddressDTO Address { get; set; }

        public decimal? PercentOwnership { get; set; }

        public int OwnerOrder { get; set; }
        public bool Acknowledge { get; set; }
    }
}
