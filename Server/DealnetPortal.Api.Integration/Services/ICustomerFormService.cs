using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICustomerFormService
    {
        CustomerLinkDTO GetCustomerLinkSettings(string dealerId);

        IList<Alert> UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId);
    }
}
