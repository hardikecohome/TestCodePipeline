using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Common.Enumeration;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Core.Services;
using DealnetPortal.Web.Infrastructure;
using DealnetPortal.Web.ServiceAgent;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class SettingsController : Controller
    {
        private readonly ISettingsManager _settingsManager;
        public SettingsController(ISettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
        }
        public async Task<FileResult> LogoImage()
        {
            var image = await _settingsManager.GetUserLogoAsync(User?.Identity?.Name);                
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

        public async Task<FileResult> Favicon()
        {
            var icon = await _settingsManager.GetUserFaviconAsync(User?.Identity?.Name);
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