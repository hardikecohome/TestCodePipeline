using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : Controller
    {
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public DealerController(IDictionaryServiceAgent dictionary)
        {
            _dictionaryServiceAgent = dictionary;
        }

        // GET: Dealer
        [HttpGet]
        public async Task<ActionResult> OnBoarding()
        {
            var model = new DealerOnboardingViewModel();

            ViewBag.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;

            return View(model);
        }

        [HttpPost]
        public ActionResult OnBoarding(DealerOnboardingViewModel model)
        {
            return View();
        }
    }
}