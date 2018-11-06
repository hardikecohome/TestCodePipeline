using System;
using System.Configuration;
using System.Globalization;
using System.Threading;
using System.Web;
using DealnetPortal.Api.Core.Helpers;
using DealnetPortal.Web.Common.Constants;

namespace DealnetPortal.Web.Common.Culture
{
    public class CultureManager : ICultureManager
    {
        private readonly string _cookieName = PortalConstants.CultureCookieName;

        public void EnsureCorrectCulture(string cultureFromRoute = null)
        {
            SetCulture(CultureHelper.FilterCulture(cultureFromRoute ?? HttpContext.Current.Request.Cookies[_cookieName]?.Value));
        }

        public void SetCulture(string culture, bool createCookie = true)
        {
            var filteredCulture = CultureHelper.FilterCulture(culture);
            SetCultureToThread(filteredCulture);

            if (createCookie)
            {
                var value = HttpContext.Current.Request.Cookies[_cookieName]?.Value;

                if (string.IsNullOrEmpty(value) || value != culture)
                {
                    CreateCookie(filteredCulture);
                }
            }            
        }        

        private void SetCultureToThread(string culture)
        {
            Thread.CurrentThread.CurrentCulture =
                     Thread.CurrentThread.CurrentUICulture = new CultureInfo(culture);
        }

        private void CreateCookie(string culture)
        {

            var cultureCookie = new HttpCookie(_cookieName)
            {
                Value = culture,
                HttpOnly = false,
                //Secure = true, Uncomment after enabling https
                Expires = DateTime.Now.AddYears(1)
            };
            HttpContext.Current?.Response?.Cookies?.Set(cultureCookie);
        }
    }
}
