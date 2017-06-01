using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICustomerFormService
    {
        CustomerLinkDTO GetCustomerLinkSettings(string dealerId);

        CustomerLinkDTO GetCustomerLinkSettingsByDealerName(string dealerName);

        CustomerLinkLanguageOptionsDTO GetCustomerLinkLanguageOptions(string hashDealerName, string language);

        IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId);

        Task<Tuple<CustomerContractInfoDTO, IList<Alert>>> SubmitCustomerFormData(CustomerFormDTO customerFormData);

        Tuple<IList<CustomerContractInfoDTO>, IList<Alert>> CustomerServiceRequest(CustomerServiceRequestDTO customerFormData);

        CustomerContractInfoDTO GetCustomerContractInfo(int contractId, string dealerName);
    }
}
