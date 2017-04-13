using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Common.Helpers;
using DealnetPortal.Web.Common.Security;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Models.Enumeration;
using DealnetPortal.Web.ServiceAgent;
using DealnetPortal.Web.ServiceAgent.Managers;

namespace DealnetPortal.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsManager _settingsManager;
        private readonly ISecurityManager _securityManager = DependencyResolver.Current.GetService<ISecurityManager>();
        public SettingsController(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }

        [OutputCache(NoStore = true, Duration = 0, Location = OutputCacheLocation.None, VaryByParam = "*")]
        public async Task<FileResult> LogoImage(string dealerName)
        {
            _securityManager.SetUserFromContext();
            BinarySettingDTO image = null;
            if (HttpContext.User.Identity.IsAuthenticated || dealerName != null)
            {
                image = await _settingsManager.GetUserLogoAsync(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : dealerName);
            }
            if (image?.ValueBytes != null)
            {
                return File(image.ValueBytes, "application/octet-stream");
            }
            //fallback:
            Stream stream; 
            switch (ApplicationSettingsManager.PortalType)
            {
                case PortalType.Ecohome:
                default:
                    stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-eco-home@2x.png"), FileMode.Open, FileAccess.Read);
                    break;
                case PortalType.Odi:
                    stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-one-dealer@2x.png"), FileMode.Open, FileAccess.Read);
                    break;
            }
            return File(stream, "image/png");
        }

        [OutputCache(NoStore = true, Duration = 0, Location = OutputCacheLocation.None, VaryByParam = "*")]
        public async Task<FileResult> Favicon()
        {
            _securityManager.SetUserFromContext();
            BinarySettingDTO icon = null;
            var dealerName = HttpRequestHelper.GetUrlReferrerRouteDataValues()?["dealerName"] as string;
            if (HttpContext.User.Identity.IsAuthenticated || dealerName != null)
            {
                icon = await _settingsManager.GetUserFaviconAsync(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : dealerName);
            }
            if (icon?.ValueBytes != null)
            {
                return File(icon.ValueBytes, "application/octet-stream");
            }
            //fallback:    
            Stream stream;
            switch (ApplicationSettingsManager.PortalType)
            {
                case PortalType.Ecohome:
                default:
                    stream = new FileStream(Server.MapPath("~/favicon.ico"), FileMode.Open, FileAccess.Read);
                    break;
                case PortalType.Odi:
                    stream = new FileStream(Server.MapPath("~/favicon-dealer.ico"), FileMode.Open, FileAccess.Read);
                    break;
            }
            return File(stream, "image/x-icon");
        }
    }
}