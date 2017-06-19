using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using DealnetPortal.Api.Common.ApiClient;
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
        private readonly IHttpApiClient _client;

        public CultureManager(IHttpApiClient client, IDictionaryServiceAgent dictionaryServiceAgent, ILoggingService loggingService)
        {
            _client = client;
            _dictionaryServiceAgent = dictionaryServiceAgent;
            _loggingService = loggingService;
        }

        public void EnsureCorrectCulture(string cultureFromRoute = null)
        {
            SetCultureToThread(CultureHelper.FilterCulture(cultureFromRoute ?? HttpContext.Current.Request.Cookies[CookieName]?.Value));
        }

        public void SetCulture(string culture, bool createCookie = true)
        {
            var filteredCulture = CultureHelper.FilterCulture(culture);
            SetCultureToThread(filteredCulture);
            if (createCookie)
            {
                CreateCookie(filteredCulture);
            }
            _client.Client.DefaultRequestHeaders.AcceptLanguage.Clear();
            _client.Client.DefaultRequestHeaders.AcceptLanguage.Add(new StringWithQualityHeaderValue(filteredCulture));
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
