using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Models;
using DealnetPortal.Web.Common.Constants;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class InfoController : Controller
    {        
        [HttpGet]
        public ActionResult Error()
        {
            var alerts = (IList<Alert>) TempData[PortalConstants.CurrentAlerts];
            return View(alerts);
        }
    }
}