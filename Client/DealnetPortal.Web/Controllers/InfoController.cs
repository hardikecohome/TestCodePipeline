using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Models;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class InfoController : Controller
    {        
        [HttpGet]
        public ActionResult Error(IList<Alert> alers)
        {
            return View();
        }
    }
}