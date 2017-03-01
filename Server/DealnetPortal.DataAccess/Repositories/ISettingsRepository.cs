using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public interface ISettingsRepository
    {
        IList<SettingValue> GetUserSettingsCollection(string dealerId);

        UserSettings GetUserSettings(string dealerId);
    }
}
