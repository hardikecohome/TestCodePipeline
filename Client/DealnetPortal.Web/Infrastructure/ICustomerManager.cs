using System.Threading.Tasks;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure
{
    public interface ICustomerManager
    {
        Task<NewCustomerViewModel> GetTemplateAsync();
        void Add(NewCustomerViewModel customer);
    }
}