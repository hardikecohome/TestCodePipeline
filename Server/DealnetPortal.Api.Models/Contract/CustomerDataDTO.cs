using System.Collections.Generic;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerDataDTO
    {
        public int Id { get; set; }
        public CustomerInfoDTO CustomerInfo { get; set; }

        public List<LocationDTO> Locations { get; set; }

        public List<PhoneDTO> Phones { get; set; }

        public List<EmailDTO> Emails { get; set; }
    }
}
