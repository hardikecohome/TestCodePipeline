using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum UserRole
    {
        Admin = 0,
        Dealer = 1,
        SubDealer = 2,
        MortgageBroker = 3,
        Customer = 4
    }
}
