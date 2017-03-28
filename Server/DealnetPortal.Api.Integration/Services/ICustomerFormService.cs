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

        CustomerLinkLanguageOptionsDTO GetCustomerLinkLanguageOptions(string dealerName, string language);

        IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId);

        Task<IList<Alert>> SubmitCustomerFormData(CustomerFormDTO customerFormData);
    }
}
