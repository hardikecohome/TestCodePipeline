using System.Collections.Generic;
using DealnetPortal.Api.Models.Aspire.AspireDb;

namespace DealnetPortal.Api.Models.Contract
{
    public class ApplicationUserDTO
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public IList<ApplicationUserDTO> SubDealers { get; set; }
        /// <summary>
        /// Sub-dealers from aspire tables
        /// </summary>
        public IList<GenericSubDealer> UdfSubDealers { get; set; }
    }
}
