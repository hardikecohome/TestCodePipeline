using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Common;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    public class SettingsController : Controller
    {
        public FileResult LogoImage()
        {
            //TODO: get image bytes from db
            //fallback:
            Stream stream; 
            switch (ApplicationSettingsManager.PortalType)
            {
                case PortalType.Ecohome:
                default:
                    stream = new FileStream(Server.MapPath("~/Content/images/logo-eco-home.png"), FileMode.Open);
                    break;
                case PortalType.Odi:
                    stream = new FileStream(Server.MapPath("~/Content/images/logo-one-dealer.png"), FileMode.Open);
                    break;
            }
            return File(stream, "image/png");
        }

        public FileResult Favicon()
        {
            //TODO: get favicon bytes from db
            //fallback:    
            Stream stream;
            switch (ApplicationSettingsManager.PortalType)
            {
                case PortalType.Ecohome:
                default:
                    stream = new FileStream(Server.MapPath("~/favicon.ico"), FileMode.Open);
                    break;
                case PortalType.Odi:
                    stream = new FileStream(Server.MapPath("~/favicon-dealer.ico"), FileMode.Open);
                    break;
            }
            return File(stream, "image/x-icon");
        }
    }
}