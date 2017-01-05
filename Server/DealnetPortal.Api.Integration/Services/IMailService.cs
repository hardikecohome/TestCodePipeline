using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;
using DealnetPortal.Api.Models.Contract;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMailService
    {
        Task<IList<Alert>> SendSubmitNotification(ContractDTO contract, string dealerEmail);
        Task<IList<Alert>> SendChangeNotification(ContractDTO contract, string dealerEmail);
    }
}
