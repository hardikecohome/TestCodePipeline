using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using DealnetPortal.Api.Common.Helpers;
using DealnetPortal.Utilities;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Core.Culture
{
    public class CultureManager : ICultureManager
    {
        private const string CookieName = "DEALNET_CULTURE_COOKIE";
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;
        private readonly ILoggingService _loggingService;

        public CultureManager(IDictionaryServiceAgent dictionaryServiceAgent, ILoggingService loggingService)
        {
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _loggingService = loggingService;
        }

        public void EnsureCorrectCulture(string cultureFromRoute = null)
        {
            SetCultureToThread(CultureHelper.FilterCulture(cultureFromRoute ?? HttpContext.Current.Request.Cookies[CookieName]?.Value));
        }

        public void SetCulture(string culture)
        {
            var filteredCulture = CultureHelper.FilterCulture(culture);
            SetCultureToThread(filteredCulture);
            CreateCookie(filteredCulture);
        }

        public async Task ChangeCulture(string culture)
        {
            try
            {
                await _dictionaryServiceAgent.ChangeDealerCulture(culture);
            }
            catch (Exception ex)
            {
                _loggingService.LogError($"Can't update culture {culture} for user {HttpContext.Current.User?.Identity?.Name}", ex);
            }
            SetCulture(culture);
        }

        private void SetCultureToThread(string culture)
        {
            Thread.CurrentThread.CurrentCulture =
                     Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        private void CreateCookie(string culture)
        {
            var cultureCookie = new HttpCookie(CookieName)
            {
                Value = culture,
                HttpOnly = true,
                //Secure = true, Uncomment after enabling https
                Expires = DateTime.Now.AddYears(1)
            };
            HttpContext.Current?.Response?.Cookies?.Set(cultureCookie);
        }
    }
}
