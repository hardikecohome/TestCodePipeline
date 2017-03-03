using System;
using System.Collections.Generic;
using System.Data.Entity;
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
            var user = _dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefault(u => u.Id == dealerId);
            if (user != null)
            {
                return user.Settings?.SettingValues?.Where(s => s.Item?.SettingType == SettingType.StringValue).ToList() ?? new List<SettingValue>();
            }
            return new List<SettingValue>();
        }

        public IList<SettingValue> GetUserBinarySettings(string dealerId)
        {
            var user = _dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefault(u => u.Id == dealerId);
            if (user != null)
            {
                return user.Settings?.SettingValues?.Where(s => s.Item?.SettingType != SettingType.StringValue).ToList() ?? new List<SettingValue>();
            }
            return new List<SettingValue>();
        }

        public SettingValue GetUserBinarySetting(SettingType settingType, string dealerId)
        {
            var user = _dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefault(u => u.Id == dealerId);
            return user?.Settings?.SettingValues?.FirstOrDefault(s => s.Item?.SettingType == settingType);
        }

        public UserSettings GetUserSettings(string dealerId)
        {
            var user = _dbContext.Users
                .Include(u => u.Settings)
                .FirstOrDefault(u => u.Id == dealerId);
            return user?.Settings;
        }
    }
}
