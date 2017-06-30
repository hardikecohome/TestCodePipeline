using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.Contract
{
    public class NewCustomerDTO
    {
        public CustomerDTO PrimaryCustomer { get; set; }
        public string CustomerComment { get; set; }
        //public DateTime? EstimatedMoveInDate { get; set; }
        public List<string> HomeImprovementTypes { get; set; }
    }
}
