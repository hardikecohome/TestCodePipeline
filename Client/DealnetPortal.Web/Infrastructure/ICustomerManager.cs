using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure
{
    public interface ICustomerManager
    {
        Task<NewCustomerViewModel> GetTemplateAsync();
        Task<IList<Alert>> AddAsync(NewCustomerViewModel customer);
        Task<IList<ClientsInformationViewModel>> GetCreatedContractsAsync();
    }
}