using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Helpers;

namespace DealnetPortal.Api.Models.Contract
{
    public class CustomerFormResponseDTO
    {
        public int ContractId { get; set; }
        public string TransactionId { get; set; }

        public string DealerName { get; set; }
        public LocationDTO DealerAdress { get; set; }
        public string DealerPhone { get; set; }
        public string DealerEmail { get; set; }

        public CreditCheckDTO CreditCheck { get; set; }
    }
}
