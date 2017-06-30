using System.Threading.Tasks;
using DealnetPortal.Api.Models.CustomerWallet;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMailСhimpService
    {
        Task AddNewSubscriberAsync(RegisterCustomerBindingModel customerData);
    }
}