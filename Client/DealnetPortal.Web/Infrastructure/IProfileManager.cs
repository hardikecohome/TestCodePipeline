using System.Threading.Tasks;
using DealnetPortal.Web.Models.MyProfile;

namespace DealnetPortal.Web.Infrastructure
{
    public interface IProfileManager
    {
        Task<ProfileViewModel> Get();
    }
}