using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface ICustomerFormServiceAgent
    {
        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerForm(CustomerFormDTO customerForm);

        Task<CustomerContractInfoDTO> GetCustomerContractInfo(int contractId, string dealerName);

        Task<CustomerLinkDTO> GetShareableLinkSettings();

        Task<IList<Alert>> UpdateShareableLinkSettings(CustomerLinkDTO customerLink);

        Task<CustomerLinkLanguageOptionsDTO> GetCustomerLinkLanguageOptions(string hashDealerName, string culture);
    }
}
