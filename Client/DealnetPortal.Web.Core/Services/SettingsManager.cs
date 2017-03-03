using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.ServiceAgent;
using Microsoft.AspNet.Identity;

namespace DealnetPortal.Web.Core.Services
{
    public class SettingsManager : ISettingsManager
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ICacheService _cacheService;
        public SettingsManager(IDictionaryServiceAgent dictionaryServiceAgent, ICacheService cacheService)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _cacheService = cacheService;
        }

        public async Task<BinarySettingDTO> GetUserFaviconAsync(string userName)
        {                                                
            return await _cacheService.GetAsync($"{PortalConstants.Favicon}{userName}", 60,
                    () => _dictionaryServiceAgent.GetDealerBinSetting(SettingType.Favicon));
            // can be changed to next, if will not work well
            //var image = await _dictionaryServiceAgent.GetDealerBinSetting(SettingType.LogoImage2X);
        }

        public async Task<BinarySettingDTO> GetUserLogoAsync(string userName)
        {
            return await _cacheService.GetAsync($"{PortalConstants.LogoImage2X}{userName}", 60,
                    () => _dictionaryServiceAgent.GetDealerBinSetting(SettingType.LogoImage2X));
        }

        public void ClearUserSettings(string userName)
        {
            _cacheService.Remove($"{PortalConstants.LogoImage2X}{userName}");
            _cacheService.Remove($"{PortalConstants.Favicon}{userName}");
        }
    }
}
