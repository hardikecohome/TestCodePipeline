using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public class SettingsRepository : BaseRepository, ISettingsRepository
    {
        public SettingsRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public IList<SettingValue> GetUserStringSettings(string dealerId)
        {            
            var user = GetUserById(dealerId);
            if (user != null)
            {
                return user.Settings?.SettingValues?.Where(s => s.Item?.SettingType == SettingType.StringValue).ToList() ?? new List<SettingValue>();
            }
            return new List<SettingValue>();
        }

        public IList<SettingValue> GetUserBinarySettings(string dealerId)
        {
            throw new NotImplementedException();
        }

        public SettingValue GetUserBinarySetting(SettingType settingType, string dealerId)
        {
            throw new NotImplementedException();
        }

        public UserSettings GetUserSettings(string dealerId)
        {
            var user = GetUserById(dealerId);
            return user?.Settings;
        }
    }
}
