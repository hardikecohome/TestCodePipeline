using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Api.Integration.Services.ESignature
{
    public interface IESignatureServiceAgent
    {
        Task<IList<Alert>> Login(string userName, string organisation, string password);

        Task<bool> Logout();
    }
}
