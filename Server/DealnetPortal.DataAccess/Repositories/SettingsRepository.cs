using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Domain;

namespace DealnetPortal.DataAccess.Repositories
{
    public class SettingsRepository : BaseRepository, ISettingsRepository
    {
        public SettingsRepository(IDatabaseFactory databaseFactory) : base(databaseFactory)
        {
        }

        public IList<SettingValue> GetUserSettingsCollection(string dealerId)
        {            
            var user = GetUserById(dealerId);
            if (user != null)
            {
                return user.Settings?.SettingValues?.ToList() ?? new List<SettingValue>();
            }
            return new List<SettingValue>();
        }

        public UserSettings GetUserSettings(string dealerId)
        {
            var user = GetUserById(dealerId);
            return user?.Settings;
        }
    }
}
