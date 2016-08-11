using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IUserManagementServiceAgent
    {
        void Logout();

        Task<bool> Register();
    }
}
