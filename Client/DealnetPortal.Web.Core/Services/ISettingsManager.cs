using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Models.UserSettings;

namespace DealnetPortal.Web.Core.Services
{
    public interface ISettingsManager
    {
        Task<BinarySettingDTO> GetUserFaviconAsync(string userName);
        Task<BinarySettingDTO> GetUserLogoAsync(string userName);
        Task<bool> CheckDealerSkinExistence(string userName);
        void ClearUserSettings(string userName);
    }
}
