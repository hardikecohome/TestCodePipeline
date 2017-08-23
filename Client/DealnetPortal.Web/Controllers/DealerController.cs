using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DealnetPortal.Web.Models.Dealer;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : Controller
    {
        // GET: Dealer
        [HttpGet]
        public ActionResult OnBoarding()
        {
            var model = new DealerOnboardingViewModel();
            model.CompanyInfo = new CompanyInfoViewModel();
            model.ProductInfo = new ProductInfoViewModel();
            model.Owners = new List<OwnerViewModel>();

            return View(model);
        }

        [HttpPost]
        public ActionResult OnBoarding(DealerOnboardingViewModel model)
        {
            return View();
        }
    }
}