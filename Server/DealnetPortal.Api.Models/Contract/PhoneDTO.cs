using DealnetPortal.Api.Common.Enumeration;

namespace DealnetPortal.Api.Models.Contract
{
    public class PhoneDTO
    {
        public int Id { get; set; }
        public PhoneType PhoneType { get; set; }

        public string PhoneNum { get; set; }

        public int CustomerId { get; set; }
    }
}
