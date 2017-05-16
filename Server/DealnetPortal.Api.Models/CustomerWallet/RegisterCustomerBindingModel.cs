using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Models.CustomerWallet
{
    public class RegisterCustomerBindingModel
    {
        public RegisterInfo RegisterInfo { get; set; }

        public CustomerProfile Profile { get; set; }

        public CustomerCreditInfo CreditInfo { get; set; }
    }
}
