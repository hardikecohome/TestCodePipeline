using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICustomerFormService
    {
        CustomerLinkDTO GetCustomerLinkSettings(string dealerId);

        CustomerLinkDTO GetCustomerLinkSettingsByDealerName(string dealerName);

        CustomerLinkLanguageOptionsDTO GetCustomerLinkLanguageOptions(string hashDealerName, string language);

        IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId);

        Task<Tuple<int?, IList<Alert>>> SubmitCustomerFormData(CustomerFormDTO customerFormData);

        CustomerContractInfoDTO GetCustomerContractInfo(int contractId, string dealerName);
    }
}
