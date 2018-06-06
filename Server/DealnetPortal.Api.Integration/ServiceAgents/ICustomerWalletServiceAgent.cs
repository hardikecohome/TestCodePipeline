using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.CustomerWallet;

namespace DealnetPortal.Api.Integration.ServiceAgents
{
    public interface ICustomerWalletServiceAgent
    {
        Task<IList<Alert>> RegisterCustomer(RegisterCustomerBindingModel registerCustomer);
        Task<IList<Alert>> CreateTransaction(TransactionInfoDTO transactionInfo);
        Task<IList<Alert>> CheckUser(string userName);
    }
}
