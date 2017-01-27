using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AuthFromContext]
    public class CalculatorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}