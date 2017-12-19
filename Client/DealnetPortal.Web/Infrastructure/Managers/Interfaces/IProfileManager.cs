using System.Collections.Generic;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Models.MyProfile;

namespace DealnetPortal.Web.Infrastructure.Managers.Interfaces
{
    public interface IProfileManager
    {
        Task<ProfileViewModel> GetDealerProfile();

        Task<IList<Alert>> UpdateDealerProfile(ProfileViewModel model);
    }
}