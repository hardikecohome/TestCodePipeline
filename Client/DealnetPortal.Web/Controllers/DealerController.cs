using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : Controller
    {
        private readonly IDealerOnBoardingManager _dealerOnBoardingManager;

        public DealerController(IDealerOnBoardingManager dealerOnBoardingManager)
        {
            _dealerOnBoardingManager = dealerOnBoardingManager;
        }

        // GET: Dealer
        [HttpGet]
        public async Task<ActionResult> OnBoarding()
        {
            return View(await _dealerOnBoardingManager.GetDealerOnBoardingFormAsynch(null));
        }

        [HttpPost]
        public ActionResult OnBoarding(DealerOnboardingViewModel model)
        {
            return View();
        }
    }
}