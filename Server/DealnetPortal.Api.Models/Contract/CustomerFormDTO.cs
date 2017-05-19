using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerFormDTO
    {
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public int? PrecreatedContractId { get; set; }
        public string PrecreatedContractTransactionId { get; set; }
        public CustomerDTO PrimaryCustomer { get; set; }
        public string CustomerComment { get; set; }
        /// <summary>
        /// Dealer service selected by customer (added as contract notes)
        /// </summary>
        public string SelectedService { get; set; }

        /// <summary>
        /// Will added as new equipment to contract
        /// </summary>
        public string SelectedServiceType { get; set; }

        public string DealUri { get; set; }
    }
}
