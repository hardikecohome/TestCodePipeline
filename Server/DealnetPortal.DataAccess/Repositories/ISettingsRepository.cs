using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface ISettingsRepository
    {
        IList<SettingValue> GetUserStringSettings(string dealerId);
        IList<SettingValue> GetUserBinarySettings(string dealerId);
        SettingValue GetUserBinarySetting(SettingType settingType, string dealerId);
        /// <summary>
        /// return user settings entry of settings of a parent user (dealer)
        /// </summary>
        /// <param name="dealerId"></param>
        /// <returns></returns>
        UserSettings GetUserSettings(string dealerId);
        bool CheckUserSkinExist(string dealerId);
    }
}
