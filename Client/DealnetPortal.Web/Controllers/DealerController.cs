using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DealnetPortal.Web.Models.Dealer;

namespace DealnetPortal.Web.Controllers
{
    public class DealerController : Controller
    {
        // GET: Dealer
        [HttpGet]
        public ActionResult OnBoarding()
        {
            return View(new DealerOnboardingViewModel());
        }

        [HttpPost]
        public ActionResult OnBoarding(DealerOnboardingViewModel model)
        {
            return View();
        }
    }
}