using System;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using DealnetPortal.Api.Core.ApiClient;
using DealnetPortal.Api.Core.Helpers;

namespace DealnetPortal.Web.Common.Culture
{
    public class CultureManager : ICultureManager
    {
        private const string CookieName = "DEALNET_CULTURE_COOKIE";

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
