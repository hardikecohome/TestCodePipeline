using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;

namespace DealnetPortal.Web.ServiceAgent
{
    public interface IUserManagementServiceAgent
    {
        Task<bool> Logout();

        Task<IList<Alert>> Register(Api.Models.RegisterBindingModel registerModel);

        Task<IList<Alert>> ChangePassword(Api.Models.ChangePasswordBindingModel changePasswordModel);

        Task<IList<Alert>> ChangePasswordAnonymously(Api.Models.ChangePasswordAnonymouslyBindingModel changePasswordModel);

        Task<IList<Alert>> ForgotPassword(Api.Models.ForgotPasswordBindingModel forgotPasswordModel);
    }
}
