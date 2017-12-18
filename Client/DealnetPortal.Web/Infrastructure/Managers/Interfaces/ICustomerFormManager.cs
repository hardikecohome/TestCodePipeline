using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;
using DealnetPortal.Web.Models;

namespace DealnetPortal.Web.Infrastructure
{
    public interface ICustomerFormManager
    {
        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormViewModel customerForm, UriBuilder urlBuilder);

        Task<SubmittedCustomerFormViewModel> GetSubmittedCustomerFormSummary(int contractId, string hashDealerName, string culture);
    }
}