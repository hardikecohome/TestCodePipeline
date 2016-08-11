using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IUserManagementServiceAgent
    {
        Task<bool> Logout();

        Task<bool> Register(DealnetPortal.Api.Models.RegisterBindingModel registerModel);

        Task<bool> ChangePassword(DealnetPortal.Api.Models.ChangePasswordBindingModel changePasswordModel);
    }
}
