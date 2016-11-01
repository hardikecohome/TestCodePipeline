using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Api.Models;

namespace DealnetPortal.Web.Controllers
{
    public class InfoController : Controller
    {        
        [HttpGet]
        public ActionResult Error(IList<Alert> alers)
        {
            return View();
        }
    }
}