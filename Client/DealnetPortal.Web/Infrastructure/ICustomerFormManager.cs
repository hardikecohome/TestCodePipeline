using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure
{
    public interface ICustomerFormManager
    {
        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitResult(CustomerFormViewModel customerForm, UriBuilder urlBuilder);

        Task<SubmittedCustomerFormViewModel> SubmittedCustomerFormViewModel(int contractId, string hashDealerName, string culture);
    }
}