using System.ComponentModel;

namespace DealnetPortal.Api.Common.Enumeration
{
    public enum UserRole
    {
        Admin = 0,
        Dealer = 1,
        SubDealer = 2,
        MortgageBroker = 3,
        [Description("Service role for new customer (client) creation")]
        CustomerCreator = 4,
        Customer = 5
    }
}
