using System;
using System.Linq;
using System.Web.Mvc;
using DealnetPortal.Web.Models.Dealer;
using DealnetPortal.Web.ServiceAgent;
using System.Threading.Tasks;
using DealnetPortal.Api.Core.Enums;
using DealnetPortal.Web.Infrastructure;

namespace DealnetPortal.Web.Controllers
{
    [AllowAnonymous]
    public class DealerController : Controller
    {
        private readonly IDealerOnBoardingManager _dealerOnBoardingManager;
        private readonly IDictionaryServiceAgent _dictionaryServiceAgent;

        public DealerController(IDealerOnBoardingManager dealerOnBoardingManager, IDictionaryServiceAgent dictionaryServiceAgent)
        {
            _dealerOnBoardingManager = dealerOnBoardingManager;
            _dictionaryServiceAgent = dictionaryServiceAgent;
        }

        [HttpGet]
        public async Task<ActionResult> OnBoarding(string onboardingLink)
        {
            return View(await _dealerOnBoardingManager.GetNewDealerOnBoardingForm(onboardingLink));
        }

        // GET: Dealer
        [HttpGet]
        public async Task<ActionResult> ResumeOnBoarding(string key)
        {
            return View("OnBoarding",await _dealerOnBoardingManager.GetDealerOnBoardingFormAsynch(key));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OnBoarding(DealerOnboardingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.DictionariesData = new DealerOnboardingDictionariesViewModel
                {
                    ProvinceTaxRates = (await _dictionaryServiceAgent.GetAllProvinceTaxRates()).Item1,
                    EquipmentTypes = (await _dictionaryServiceAgent.GetAllEquipmentTypes()).Item1?.OrderBy(x => x.Description).ToList()
                };

                return View(model);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveDraft(DealerOnboardingViewModel model)
        {
            var result = await _dealerOnBoardingManager.SaveDraft(model);
            var modal = new SaveAndResumeViewModel
                        {
                            AccessKey = result.Item1?.AccessKey ?? model.AccessKey,
                            Success = result.Item2 != null && result.Item2.Any(a => a.Type == AlertType.Error),
                            Alerts = result.Item2?.Where(a=>a.Type == AlertType.Error),
                            Email = model.Owners.Any()
                                        ? model.Owners.First().EmailAddress
                                        : string.IsNullOrEmpty(model.CompanyInfo.EmailAddress)
                                            ? model.CompanyInfo.EmailAddress
                                            : String.Empty
                        };
            return PartialView("OnBoarding/_SaveAndResumeModal", modal);
        }
    }
}