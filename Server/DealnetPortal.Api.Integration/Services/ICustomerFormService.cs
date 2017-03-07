using System.Security.Cryptography.X509Certificates;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICustomerFormService
    {
        CustomerLinkDTO GetCustomerLinkSettings(string dealerId);

        void UpdateCustomerLinkSettings(CustomerLinkDTO customerLinkSettings, string dealerId);
    }
}
