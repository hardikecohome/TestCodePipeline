using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IUserManagementServiceAgent
    {
        Task<bool> Logout();

        Task<IList<Alert>> Register(DealnetPortal.Api.Models.RegisterBindingModel registerModel);

        Task<IList<Alert>> ChangePassword(DealnetPortal.Api.Models.ChangePasswordBindingModel changePasswordModel);

        Task<IList<Alert>> ChangePasswordAnonymously(DealnetPortal.Api.Models.ChangePasswordAnonymouslyBindingModel changePasswordModel);

        Task<IList<Alert>> ForgotPassword(DealnetPortal.Api.Models.ForgotPasswordBindingModel forgotPasswordModel);
    }
}
