using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Core.Types;
using DealnetPortal.Api.Models;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;

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
    }
}