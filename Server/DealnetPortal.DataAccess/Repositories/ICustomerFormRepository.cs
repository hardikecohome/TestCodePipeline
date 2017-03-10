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
        CustomerLink UpdateCustomerLinkLanguages(ICollection<Language> enabledLanguages, string dealerId);
        CustomerLink UpdateCustomerLinkServices(ICollection<DealerService> dealerServices, string dealerId);
    }
}
