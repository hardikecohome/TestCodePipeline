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
        public ActionResult PrivacyPolicy()
        {
            return View();
        }
    }
}