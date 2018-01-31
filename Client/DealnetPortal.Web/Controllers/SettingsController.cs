using DealnetPortal.Api.Models.UserSettings;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.Infrastructure.Managers;
using DealnetPortal.Web.Models.Enumeration;

using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.UI;
using DealnetPortal.Api.Core.Helpers;
using DealnetPortal.Web.Common.Culture;
using DealnetPortal.Web.Infrastructure.Managers.Interfaces;
using System.Security.Claims;
using DealnetPortal.Utilities.Logging;
using log4net;

namespace DealnetPortal.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettingsManager _settingsManager;

        private readonly ILoggingService _loggingService;

        public SettingsController(ISettingsManager settingsManager, ILoggingService loggingService)
        {
            _settingsManager = settingsManager;
            _loggingService = loggingService;
        }

        [OutputCache(NoStore = true, Duration = 0, Location = OutputCacheLocation.None, VaryByParam = "*")]
        public async Task<FileResult> LogoImage(string hashDealerName)
        {
            BinarySettingDTO image = null;
            if (HttpContext.User.Identity.IsAuthenticated || hashDealerName != null)
            {
                _loggingService.LogInfo($"Get dealer Logo for dealer: {(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : hashDealerName)}");
                image = await _settingsManager.GetUserLogoAsync(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : hashDealerName);
            }
            if (image?.ValueBytes != null)
            {
                _loggingService.LogInfo($"Got dealer Logo {image?.ValueBytes.Length}bytes settings for dealer: {(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : hashDealerName)}");
                return File(image.ValueBytes, "image/png");
            }
            //fallback:
            Stream stream;
            if ((CultureHelper.CurrentCultureType != CultureType.French && string.IsNullOrEmpty(User?.Identity?.Name)) || (!string.IsNullOrEmpty(User?.Identity?.Name) && !((bool)((ClaimsPrincipal)User)?.HasClaim("QuebecDealer", "True"))))
            {
                switch (ApplicationSettingsManager.PortalType)
                {
                    case PortalType.Ecohome:
                    default:
                        stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-eco-home@2x.png"),
                            FileMode.Open, FileAccess.Read);
                        break;
                    case PortalType.Odi:
                        stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-one-dealer@2x.png"),
                            FileMode.Open, FileAccess.Read);
                        break;
                }
            }
            else
            {
                switch (ApplicationSettingsManager.PortalType)
                {
                    case PortalType.Ecohome:
                    default:
                        stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-eco-home-fr@2x.png"),
                            FileMode.Open, FileAccess.Read);
                        break;
                    case PortalType.Odi:
                        stream = new FileStream(Server.MapPath("~/Content/images/logos/logo-one-dealer@2x.png"),
                            FileMode.Open, FileAccess.Read);
                        break;
                }
            }
            return File(stream, "image/png");
        }

        [OutputCache(NoStore = true, Duration = 0, Location = OutputCacheLocation.None, VaryByParam = "*")]
        public async Task<FileResult> Favicon(string hashDealerName)
        {
            BinarySettingDTO icon = null;
            if (HttpContext.User.Identity.IsAuthenticated || hashDealerName != null)
            {
                icon = await _settingsManager.GetUserFaviconAsync(!string.IsNullOrEmpty(User?.Identity?.Name) ? User.Identity.Name : hashDealerName);
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