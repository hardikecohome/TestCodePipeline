using DealnetPortal.Api.Core.Helpers;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Web.Common.Constants;

using System.Collections.Generic;
using System.Web.Mvc;

namespace DealnetPortal.Web.Controllers
{
    public class InfoController : Controller
    {        
        [HttpGet]
        [Authorize]
        public ActionResult Error()
        {
            var alerts = (IList<Alert>) TempData[PortalConstants.CurrentAlerts];
            return View(alerts);
        }

        [HttpGet]
        public ActionResult AnonymousError()
        {
            return View();
        }

        [HttpGet]
        public ActionResult PrivacyPolicy(bool? IsQuebecDealer)
        {
            ViewBag.IsQuebecDealer = IsQuebecDealer ?? false;
            if (Request.UrlReferrer != null)
            {
                ViewBag.AdditionalNavbarClasses = Request.UrlReferrer.LocalPath.ToUpper().Contains("ONBOARD") ? "onboard-navbar" : "";
                if (Request.UrlReferrer.LocalPath.ToUpper().Contains("ONBOARD") && CultureHelper.CurrentCultureType == CultureType.French)
                {
                    ViewBag.IsQuebecDealer = true;
                }
            }
            
            return View();
        }
    }
}