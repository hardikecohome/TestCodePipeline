using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Web.Models;

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
            model.CompanyInfo = new CompanyInfoViewModel();
            model.ProductInfo = new ProductInfoViewModel();
            model.Owners = new List<OwnerViewModel>();

            ViewBag.ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1;
            ViewBag.EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1;

            return View(model);
        }

        [HttpPost]
        public ActionResult OnBoarding(DealerOnboardingViewModel model)
        {
            return View();
        }
    }
}