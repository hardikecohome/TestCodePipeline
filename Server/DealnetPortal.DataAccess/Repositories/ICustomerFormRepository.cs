using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface ICustomerFormRepository
    {
        CustomerLink GetCustomerLinkSettings(string dealerId);
        CustomerLink GetCustomerLinkSettingsByDealerName(string dealerName);
        CustomerLink GetCustomerLinkSettingsByHashDealerName(string hashDealerName);
        CustomerLink UpdateCustomerLinkLanguages(ICollection<DealerLanguage> enabledLanguages, string dealerId);
        CustomerLink UpdateCustomerLinkServices(ICollection<DealerService> dealerServices, string dealerId);
        Contract AddCustomerContractData(int contractId, string commentsHeader, string selectedService, string customerComment, string dealerId);        
    }
}
