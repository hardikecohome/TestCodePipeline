using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    //Request from CustomerWallet with new Improvement
    public class CustomerServiceRequestDTO
    {
        public string DealerId { get; set; }
        public string DealerName { get; set; }
        public int? PrecreatedContractId { get; set; }
        public string PrecreatedContractTransactionId { get; set; }
        public CustomerDTO PrimaryCustomer { get; set; }
        public string CustomerComment { get; set; }        
        public IList<string> SelectedServices { get; set; }        
    }
}
