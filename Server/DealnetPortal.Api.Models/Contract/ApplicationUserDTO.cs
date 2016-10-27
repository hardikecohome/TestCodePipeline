using System.Collections.Generic;

namespace DealnetPortal.Api.Models.Contract
{
    public class ApplicationUserDTO
    {
        public string Id { get; set; }
        public string SubDealerName { get; set; }
        public string UserName { get; set; }
        public IList<ApplicationUserDTO> SubDealers { get; set; }
    }
}
