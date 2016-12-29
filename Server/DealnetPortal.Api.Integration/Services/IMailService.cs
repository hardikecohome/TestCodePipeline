using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Api.Integration.Services
{
    public interface IMailService
    {
        Task<IList<Alert>> SendSubmitNotification(int contractId, string contractOwnerId);
        Task<IList<Alert>> SendChangeNotification(int contractId, string contractOwnerId);
    }
}
