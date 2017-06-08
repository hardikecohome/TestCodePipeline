using System.Threading.Tasks;
using DealnetPortal.Api.Models.UserSettings;

namespace DealnetPortal.Web.Infrastructure.Managers
{
    public interface ISettingsManager
    {
        Task<BinarySettingDTO> GetUserFaviconAsync(string userName);
        Task<BinarySettingDTO> GetUserLogoAsync(string userName);
        Task<bool> CheckDealerSkinExistence(string userName);
        void ClearUserSettings(string userName);
    }
}
