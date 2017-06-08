using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;

namespace DealnetPortal.Api.Integration.Services
{
    public interface ICustomerWalletService
    {
        Task<IList<Alert>> CreateCustomerByContract(DealnetPortal.Domain.Contract contract, string contractOwnerId);
    }
}
